using Zylance.Contract.Models.Vault;
using Zylance.Core.Vault.Managers;

namespace Zylance.Core.Vault.Interfaces;

/// <summary>
///     Represents a vault exposing managers for accounts, ledgers, and metadata,
///     and helpers for creating and executing work within a transactional scope.
/// </summary>
public interface IVault
{
    /// <summary>
    ///     Unique identifier for the vault instance.
    /// </summary>
    public Guid VaultId { get; }

    /// <summary>
    ///     Whether the vault is currently locked.
    /// </summary>
    public bool Locked { get; }

    /// <summary>
    ///     Access to account manager for CRUD operations on accounts.
    /// </summary>
    public IAccountManager Accounts { get; }

    /// <summary>
    ///     Access to ledger manager for CRUD and search operations on ledger entries.
    /// </summary>
    public ILedgerManager Ledgers { get; }

    /// <summary>
    ///     Access to vault metadata key/value store.
    /// </summary>
    public IMetadataManager Metadata { get; }

    /// <summary>
    ///     Creates a transactional scope for performing multiple vault operations
    ///     atomically.
    /// </summary>
    public IVaultScope CreateScope();

    /// <summary>
    ///     Executes an action within a vault scope, ensuring disposal.
    /// </summary>
    public Task WithScope(Func<IVaultScope, Task> action);

    /// <summary>
    ///     Converts the vault to a serializable reference object.
    /// </summary>
    public VaultRef ToRef()
    {
        return new VaultRef { Id = VaultId.ToString(), Locked = Locked };
    }
}
