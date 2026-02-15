using Serilog;
using Zylance.Contract;
using Zylance.Contract.Api.File;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Importers.Models;
using Zylance.Core.Importers.Ofx;
using Zylance.Core.Router.Attributes;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller handling import actions (e.g., Import:Start).
/// </summary>
[Controller]
public class ImportController(FileService fileService, ZylanceCore zylance, VaultContext vaultContext)
{
    /// <summary>
    ///     Handles the Import:Start action, starting a new import process.
    /// </summary>
    [RequestHandler]
    public async Task StartImport(ZyRequest<StartImportReq> req, ZyResponse<StartImportRes> res)
    {
        var cancellationSource = new CancellationTokenSource();

        var fileRef = req.Data.FileRef;
        if (!await fileService.Exists(fileRef))
            throw new FileNotFoundException($"File not found: {fileRef}");

        var importId = Guid.CreateVersion7().ToString();
        res.SetData(new StartImportRes { ImportId = importId });
        res.Send();

        _ = zylance
            .Gateway.ObserveEvent<ImportCancelledEvt>(ZylanceEvents.Import_Cancelled)
            .Select(evt => evt.ImportId == importId)
            .TakeFirstAsync(cancellationSource.Token)
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
            zylance.Gateway.SendEvent(new ImportStartedEvt { ImportId = importId });
            var result = await PerformImport(importId, accounts, transactions);
            zylance.Gateway.SendEvent(
                new ImportFinishedEvt
                {
                    ImportId = importId,
                    NumAccountsImported = result.NumAccountsImported,
                    NumTransactionsSkipped = result.NumTransactionsSkipped,
                    NumTransactionsImported = result.NumTransactionsImported,
                }
            );
        }
        catch (Exception e)
        {
            zylance.Gateway.SendEvent(new ImportErrorEvt { ImportId = importId, ErrorMessage = e.Message });
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
            evtData.Accounts.AddRange(accounts);
            zylance.Gateway.Send(MessageUtils.ToEventPayload(evtData));

            accounts = await zylance
                .Gateway.ObserveEvent<ImportSetAccountsEvt>(ZylanceEvents.Import_SetAccounts)
                .Where(evt => evt.ImportId == importId)
                .Select(evt => evt.Accounts)
                .TakeFirstAsync(cancellationToken);

            // TODO: validate accounts (e.g., missing required fields, etc.)
            accountsValid = true;
        }

        return [.. accounts.Select(AccountModel.FromData)];
    }

    private void ReportProgress(string importId, int progress, int total)
    {
        var percent = (float)progress / total * 100;
        var evt = new ImportProgressEvt { ImportId = importId, Progress = percent };
        zylance.Gateway.Send(MessageUtils.ToEventPayload(evt));
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

        var vault = vaultContext.ActiveVaultOrThrow;
        await vault.WithScope(async trxVault =>
        {
            ReportProgress(importId, completedTasks, totalTasks);
            foreach (var model in accounts)
            {
                await trxVault.Accounts.SaveAsync(model);
                ReportProgress(importId, ++completedTasks, totalTasks);
            }

            var trxIds = transactions.Where(t => t.TrxId is not null).Select(t => t.TrxId!).Distinct().ToList();
            var existingTransactions = await trxVault.Ledgers.FindByTrxIdsAsync(trxIds);
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
                    await trxVault.Ledgers.SaveAsync(transaction);
                }

                ReportProgress(importId, ++completedTasks, totalTasks);
            }
        });

        return new()
        {
            NumAccountsImported = accounts.Count,
            NumTransactionsImported = transactionsImported,
            NumTransactionsSkipped = transactionsSkipped,
        };
    }
}
