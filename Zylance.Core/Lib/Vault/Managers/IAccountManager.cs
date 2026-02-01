using Zylance.Contract.Models.Account;

namespace Zylance.Core.Lib.Vault.Managers;

public interface IAccountManager : IRecordManager<Guid, AccountData>
{
    public Task<CursorList<AccountData>> ListAsync();
}
