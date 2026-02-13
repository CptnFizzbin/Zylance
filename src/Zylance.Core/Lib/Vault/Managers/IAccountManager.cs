using Zylance.Contract.Models.Account;

namespace Zylance.Core.Lib.Vault.Managers;

/// <summary>
/// Manager for account records in a vault.
/// Provides CRUD and listing operations for accounts.
/// </summary>
public interface IAccountManager : IRecordManager<Guid, AccountData>
{
    /// <summary>
    /// Returns a paged list of accounts.
    /// </summary>
    public Task<CursorList<AccountData>> ListAsync();
}
