using Zylance.Contract.Models.Vault;
using Zylance.Core.Lib.Vault.Managers;

namespace Zylance.Core.Lib.Vault;

public interface IVault
{
    public Guid VaultId { get; }

    public bool Locked { get; }

    public IAccountManager Accounts { get; }
    public ILedgerManager Ledgers { get; }
    public IMetadataManager Metadata { get; }

    public IVaultScope CreateScope();
    public Task WithScope(Func<IVaultScope, Task> action);

    public VaultRef ToRef()
    {
        return new VaultRef { Id = VaultId.ToString(), Locked = Locked };
    }
}
