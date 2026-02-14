using Zylance.Contract.Api.Ledger;
using Zylance.Contract.Models.Ledger;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Vault.Managers;

/// <summary>
///     Manager for ledger entries in a vault.
///     Supports listing and searching ledger entries.
/// </summary>
public interface ILedgerManager : IRecordManager<Guid, LedgerEntryData>
{
    /// <summary>
    ///     Lists ledger entries optionally filtered by <paramref name="filter" />.
    /// </summary>
    public Task<CursorList<LedgerEntryData>> ListAsync(LedgerFilter? filter);

    /// <summary>
    ///     Searches ledger entries using a text query and optional filter.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="filter">Optional filter to narrow results.</param>
    public Task<CursorList<LedgerEntryData>> SearchAsync(string searchText, LedgerFilter? filter);
}
