using Zylance.Core.Lib.Vault.Managers;

namespace Zylance.Core.Lib.Vault;

public interface IVault
{
    public IAccountManager Accounts { get; }
    public ILedgerManager Ledgers { get; }

    public IVaultScope CreateScope();
    public Task WithScope(Func<IVaultScope, Task> action);
}
