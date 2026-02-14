using Zylance.Contract.Api.Ledger;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Vault.Managers;

/// <summary>
///     Manager for ledger entries in a vault.
///     Supports listing and searching ledger entries.
/// </summary>
public interface ILedgerManager : IRecordManager<Guid, LedgerEntryModel>
{
    /// <summary>
    ///     Lists ledger entries optionally filtered by <paramref name="filter" />.
    /// </summary>
    public Task<CursorList<LedgerEntryModel>> ListAsync(LedgerFilter? filter);

    /// <summary>
    ///     Searches ledger entries using a text query and optional filter.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="filter">Optional filter to narrow results.</param>
    public Task<CursorList<LedgerEntryModel>> SearchAsync(string searchText, LedgerFilter? filter);
}
