using Zylance.Contract.Api.Ledger;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.App.Controllers;

[Controller]
public class LedgerController(VaultService vaultService)
{
    [RequestHandler]
    public async Task CreateLedgerEntry(ZyRequest<CreateLedgerEntryReq> req, ZyResponse<CreateLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        await vault.WithScope(async scope =>
        {
            var savedEntry = await scope.Vault.Ledgers.SaveAsync(data.Entry);
            res.SetData(new CreateLedgerEntryRes { Entry = savedEntry });
        });
    }

    [RequestHandler]
    public async Task GetLedgerEntry(ZyRequest<GetLedgerEntryReq> req, ZyResponse<GetLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out var entryId))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        var entry = await vault.Ledgers.GetAsync(entryId);
        res.SetData(new GetLedgerEntryRes { Entry = entry });
    }

    [RequestHandler]
    public async Task ListLedgerEntries(ZyRequest<ListLedgerEntriesReq> req, ZyResponse<ListLedgerEntriesRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        var result = await vault.Ledgers.ListAsync(data.Filter);

        res.SetData(
            new ListLedgerEntriesRes
            {
                TotalCount = result.TotalCount,
                Cursor = result.NextCursor,
                LastPage = result.IsLastPage,
                Entries = { result.Items },
            }
        );
    }

    [RequestHandler]
    public async Task UpdateLedgerEntry(ZyRequest<UpdateLedgerEntryReq> req, ZyResponse<UpdateLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out var entryId))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        // Ensure the entry ID matches the request ID
        if (data.Entry.Id != data.Id)
            throw new ArgumentException("Entry ID mismatch between URL and payload");

        await vault.WithScope(async scope =>
        {
            var updatedEntry = await scope.Vault.Ledgers.SaveAsync(data.Entry);
            res.SetData(new UpdateLedgerEntryRes { Entry = updatedEntry });
        });
    }

    [RequestHandler]
    public async Task DeleteLedgerEntry(ZyRequest<DeleteLedgerEntryReq> req, ZyResponse<DeleteLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out var entryId))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        await vault.WithScope(async scope =>
        {
            await scope.Vault.Ledgers.DeleteAsync(entryId);
            res.SetData(new DeleteLedgerEntryRes { Success = true });
        });
    }

    [RequestHandler]
    public async Task SearchLedgerEntries(ZyRequest<SearchLedgerEntriesReq> req, ZyResponse<SearchLedgerEntriesRes> res)
    {
        var data = req.GetData();
        var vault = vaultService.ActiveVault ?? throw VaultException.NoActiveVault();

        var searchText = data.Query ?? string.Empty;
        var result = await vault.Ledgers.SearchAsync(searchText, data.Filter);

        res.SetData(
            new SearchLedgerEntriesRes
            {
                TotalCount = result.TotalCount,
                Cursor = result.NextCursor,
                LastPage = result.IsLastPage,
                Entries = { result.Items },
            }
        );
    }
}
