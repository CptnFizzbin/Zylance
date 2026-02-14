using Zylance.Contract.Api.Ledger;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Router.Attributes;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Exceptions;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller that handles ledger-related requests
///     (create/read/update/delete/search).
/// </summary>
[Controller]
public class LedgerController(VaultContext vaultContext)
{
    /// <summary>
    ///     Creates a new ledger entry in the active vault.
    /// </summary>
    /// <param name="req">Request containing the ledger entry to create.</param>
    /// <param name="res">Response to populate with the created entry.</param>
    [RequestHandler]
    public async Task CreateLedgerEntry(ZyRequest<CreateLedgerEntryReq> req, ZyResponse<CreateLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        await vault.WithScope(async scope =>
        {
            var model = LedgerEntryModel.FromData(data.Entry);
            var savedEntry = await scope.Vault.Ledgers.SaveAsync(model);
            res.SetData(new CreateLedgerEntryRes { Entry = LedgerEntryModel.ToData(savedEntry) });
        });
    }

    /// <summary>
    ///     Retrieves a ledger entry by id from the active vault.
    /// </summary>
    /// <param name="req">Request containing the ledger entry id.</param>
    /// <param name="res">Response to populate with the found entry.</param>
    [RequestHandler]
    public async Task GetLedgerEntry(ZyRequest<GetLedgerEntryReq> req, ZyResponse<GetLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out var entryId))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        var entry = await vault.Ledgers.GetAsync(entryId);
        res.SetData(new GetLedgerEntryRes { Entry = LedgerEntryModel.ToData(entry) });
    }

    /// <summary>
    ///     Lists ledger entries with optional filtering and pagination.
    /// </summary>
    /// <param name="req">Request containing filter and pagination parameters.</param>
    /// <param name="res">Response to populate with the paginated results.</param>
    [RequestHandler]
    public async Task ListLedgerEntries(ZyRequest<ListLedgerEntriesReq> req, ZyResponse<ListLedgerEntriesRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        var result = await vault.Ledgers.ListAsync(data.Filter);
        var resData = new ListLedgerEntriesRes
        {
            TotalCount = result.TotalCount,
            Cursor = result.Cursor,
            LastPage = !result.HasNextPage,
        };
        resData.Entries.AddRange([.. result.Items.Select(LedgerEntryModel.ToData)]);

        res.SetData(resData);
    }

    /// <summary>
    ///     Updates an existing ledger entry in the active vault.
    /// </summary>
    /// <param name="req">Request containing the updated ledger entry.</param>
    /// <param name="res">Response to populate with the updated entry.</param>
    [RequestHandler]
    public async Task UpdateLedgerEntry(ZyRequest<UpdateLedgerEntryReq> req, ZyResponse<UpdateLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out _))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        // Ensure the entry ID matches the request ID
        if (data.Entry.Id != data.Id)
            throw new ArgumentException("Entry ID mismatch between URL and payload");

        await vault.WithScope(async scope =>
        {
            var model = LedgerEntryModel.FromData(data.Entry);
            var updatedEntry = await scope.Vault.Ledgers.SaveAsync(model);
            res.SetData(new UpdateLedgerEntryRes { Entry = LedgerEntryModel.ToData(updatedEntry) });
        });
    }

    /// <summary>
    ///     Deletes a ledger entry by id from the active vault.
    /// </summary>
    /// <param name="req">Request containing the id of the entry to delete.</param>
    /// <param name="res">Response to indicate success/failure.</param>
    [RequestHandler]
    public async Task DeleteLedgerEntry(ZyRequest<DeleteLedgerEntryReq> req, ZyResponse<DeleteLedgerEntryRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        if (!Guid.TryParse(data.Id, out var entryId))
            throw new ArgumentException($"Invalid ledger entry ID format: {data.Id}");

        await vault.WithScope(async scope =>
        {
            await scope.Vault.Ledgers.DeleteAsync(entryId);
            res.SetData(new DeleteLedgerEntryRes { Success = true });
        });
    }

    /// <summary>
    ///     Searches ledger entries by text and optional filters.
    /// </summary>
    /// <param name="req">Request containing the search query and filters.</param>
    /// <param name="res">Response to populate with the search results.</param>
    [RequestHandler]
    public async Task SearchLedgerEntries(ZyRequest<SearchLedgerEntriesReq> req, ZyResponse<SearchLedgerEntriesRes> res)
    {
        var data = req.GetData();
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        var searchText = data.Query ?? string.Empty;
        var result = await vault.Ledgers.SearchAsync(searchText, data.Filter);
        var resData = new SearchLedgerEntriesRes
        {
            TotalCount = result.TotalCount,
            Cursor = result.Cursor,
            LastPage = !result.HasNextPage,
        };
        resData.Entries.AddRange([.. result.Items.Select(LedgerEntryModel.ToData)]);

        res.SetData(resData);
    }
}
