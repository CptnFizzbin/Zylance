using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Serilog;
using Zylance.Contract.Api.File;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Importers.Models;
using Zylance.Core.Importers.Ofx;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller handling import actions (e.g., Import:Start).
/// </summary>
[Controller]
public class ImportController(FileService fileService, GatewayService gateway, VaultContext vaultContext)
{
    private static readonly ILogger Log = ZyLogger.ForContext<ImportController>();

    /// <summary>
    ///     Handles the Import:Start action, starting a new import process.
    /// </summary>
    [RequestHandler]
    public async Task StartImport(ZyRequest<StartImportReq> req, ZyResponse<StartImportRes> res)
    {
        var cancellationSource = new CancellationTokenSource();

        var fileRef = req.Data.FileRef;
        Log.Information("StartImport called for FileRef={FileRef}", fileRef);
        if (!await fileService.Exists(fileRef))
            throw new FileNotFoundException($"File not found: {fileRef}");

        var importId = Guid.CreateVersion7().ToString();
        res.SetData(new StartImportRes { ImportId = importId });
        res.Send();

        _ = gateway
            .ObserveEvent<ImportCancelledEvt>()
            .Select(zyEvt => zyEvt.Data)
            .Select(evt => evt.ImportId == importId)
            .ToTask(cancellationSource.Token)
            .ContinueWith(
                _ =>
                {
                    Log.Information("Import cancelled, triggering cancellation token.");
                    return cancellationSource.CancelAsync();
                },
                cancellationSource.Token
            );

        // TODO: find the right importer based on the file extension and call it to process the file
        var importer = new OfxImportParser();

        var results = await fileService.WithFileAsync(
            fileRef,
            fileStream => importer.ParseAsync(fileStream, cancellationSource.Token)
        );

        var accounts = await GetAccountData(importId, results.Statements, cancellationSource.Token);

        var transactions = results.Statements.SelectMany(s => s.Transactions).ToList();

        try
        {
            gateway.SendEvent(new ImportStartedEvt { ImportId = importId });
            var result = await PerformImport(importId, accounts, transactions);
            gateway.SendEvent(
                new ImportFinishedEvt
                {
                    ImportId = importId,
                    NumAccountsImported = result.NumAccountsImported,
                    NumTransactionsSkipped = result.NumTransactionsSkipped,
                    NumTransactionsImported = result.NumTransactionsImported,
                }
            );
            Log.Information("Import {ImportId} finished successfully", importId);
        }
        catch (Exception e)
        {
            Log.Error(e, "Import {ImportId} failed", importId);
            gateway.SendEvent(new ImportErrorEvt { ImportId = importId, ErrorMessage = e.Message });
            throw;
        }
    }

    private async Task<List<AccountModel>> GetAccountData(
        string importId,
        List<ImportStatement> statements,
        CancellationToken cancellationToken = default
    )
    {
        var vault = vaultContext.ActiveVaultOrThrow;
        var knownAccounts = (await vault.Accounts.ListAsync()).ToDictionary(a => a.Id, a => a);

        var accounts = statements
            .Select(s => s.Account)
            .DistinctBy(a => a.Id)
            .Select(a =>
            {
                var accountName = knownAccounts.TryGetValue(a.Id, out var knownAccount)
                    ? knownAccount.Name
                    : string.Empty;

                return a with
                {
                    Name = accountName,
                };
            })
            .Select(AccountModel.ToData);

        var accountsValid = false;
        while (!accountsValid)
        {
            var evtData = new ImportGetAccountsEvt { ImportId = importId };
            var accountData = accounts.ToList();
            evtData.Accounts.AddRange(accountData);
            Log.Debug(
                "Requesting account selection for ImportId={ImportId} with {Count} candidate accounts",
                importId,
                accountData.Count
            );
            gateway.Send(MessageUtils.ToEventPayload(evtData));

            accounts = await gateway
                .ObserveEvent<ImportSetAccountsEvt>()
                .Select(zyEvt => zyEvt.Data)
                .Where(evt => evt.ImportId == importId)
                .Take(1)
                .Select(evt => evt.Accounts)
                .ToTask(cancellationToken);

            // TODO: validate accounts (e.g., missing required fields, etc.)
            accountsValid = true;
        }

        return [.. accounts.Select(AccountModel.FromData)];
    }

    private void ReportProgress(string importId, int progress, int total)
    {
        var percent = (float)progress / total * 100;
        var evt = new ImportProgressEvt { ImportId = importId, Progress = percent };
        Log.Debug("Import {ImportId} progress {Progress}/{Total} ({Percent}%)", importId, progress, total, percent);
        gateway.Send(MessageUtils.ToEventPayload(evt));
    }

    private async Task<ImportResult> PerformImport(
        string importId,
        List<AccountModel> accounts,
        List<LedgerEntryModel> transactions
    )
    {
        var totalTasks = accounts.Count + transactions.Count;
        var completedTasks = 0;
        var transactionsImported = 0;
        var transactionsSkipped = 0;

        Log.Information(
            "Performing import {ImportId} with {Accounts} accounts and {Transactions} transactions",
            importId,
            accounts.Count,
            transactions.Count
        );
        var vault = vaultContext.ActiveVaultOrThrow;
        await vault.WithScope(async scope =>
        {
            ReportProgress(importId, completedTasks, totalTasks);
            foreach (var model in accounts)
            {
                await scope.Vault.Accounts.SaveAsync(model);
                ReportProgress(importId, ++completedTasks, totalTasks);
                Log.Debug("Saved account for Import {ImportId}: {AccountId}", importId, model.Id);
            }

            var trxIds = transactions.Where(t => t.TrxId is not null).Select(t => t.TrxId!).Distinct().ToList();
            var existingTransactions = await scope.Vault.Ledgers.FindByTrxIdsAsync(trxIds);
            var existingTrxIds = existingTransactions.Select(t => t.TrxId).ToHashSet();

            foreach (var transaction in transactions)
            {
                if (existingTrxIds.Contains(transaction.TrxId))
                {
                    transactionsSkipped++;
                }
                else
                {
                    transactionsImported++;
                    await scope.Vault.Ledgers.SaveAsync(transaction);
                }

                ReportProgress(importId, ++completedTasks, totalTasks);
                Log.Debug("Saved transaction for Import {ImportId}: {TransactionId}", importId, transaction.Id);
            }
        });

        return new ImportResult
        {
            NumAccountsImported = accounts.Count,
            NumTransactionsImported = transactionsImported,
            NumTransactionsSkipped = transactionsSkipped,
        };
    }
}
