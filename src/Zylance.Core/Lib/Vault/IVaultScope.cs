namespace Zylance.Core.Lib.Vault;

/// <summary>
/// Represents a transactional scope for operating on an <see cref="IVault"/>.
/// Commit or rollback must be called (or the scope disposed) to finalize changes.
/// </summary>
public interface IVaultScope : IAsyncDisposable
{
    /// <summary>
    /// The vault instance associated with this scope.
    /// </summary>
    public IVault Vault { get; }

    /// <summary>
    /// Commits all pending changes within the scope.
    /// </summary>
    public Task Commit();

    /// <summary>
    /// Rolls back all pending changes within the scope.
    /// </summary>
    public Task Rollback();
}
