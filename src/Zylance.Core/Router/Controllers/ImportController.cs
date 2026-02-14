using System.Globalization;
using Zylance.Contract.Api.File;
using Zylance.Contract.Models.Account;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Importers.Ofx;
using Zylance.Core.Router.Attributes;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Exceptions;

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
        var cancellationToken = new CancellationToken(false);
        var importId = Guid.CreateVersion7().ToString();
        var vault = vaultContext.ActiveVault;

        if (vault?.Locked ?? true)
            throw VaultException.VaultLocked();

        var fileRef = req.Data.FileRef;

        // TODO: Implement actual import logic and generate a real import_id
        res.SetData(new StartImportRes { ImportId = importId });
        res.Send();

        // TODO: find the right importer based on the file extension and call it to process the file
        var importer = new OfxImportParser();

        var fileStream = await fileService.OpenFileAsync(fileRef);
        var statements = await importer.ParseAsync(fileStream, cancellationToken);
        var knownAccounts = (await vault.Accounts.ListAsync()).ToDictionary(a => a.Id, a => a);

        var accounts = statements
            .Statements.Select(s => s.Account)
            .DistinctBy(a => a.Id)
            .Select(a =>
            {
                var accountName = knownAccounts.TryGetValue(a.Id, out var knownAccount)
                    ? knownAccount.Name
                    : string.Empty;

                return new AccountData
                {
                    Id = a.Id,
                    Name = accountName,
                    Type = a.Type,
                    Currency = a.Currency,
                    Balance = a.Balance.ToString(CultureInfo.InvariantCulture),
                    AvailableBalance = a.AvailableBalance?.ToString(CultureInfo.InvariantCulture),
                };
            })
            .ToArray();

        var evtData = new ImportGetAccountsEvt { ImportId = importId };
        evtData.Accounts.AddRange(accounts);

        zylance.Gateway.Send(MessageUtils.ToEventPayload(evtData));
    }
}
