using Microsoft.EntityFrameworkCore.Storage;
using Zylance.Core.Lib.Vault;

namespace Zylance.Vault.Local;

/// <summary>
///     Local implementation of IVaultScope that provides transactional scope for vault operations.
///     Why use a scope pattern? This implements the Unit of Work pattern, which allows multiple
///     operations to be grouped together and committed or rolled back as a single transaction.
///     This ensures data consistency - either all changes succeed or none do.
/// </summary>
public class LocalVaultScope(LocalVault vault, IDbContextTransaction transaction) : IVaultScope
{
    private bool _disposed;

    public IVault Vault { get; } = vault;

    /// <summary>
    ///     Commits all changes made within this scope to the database.
    ///     Why async? Database operations are I/O bound, and async/await allows the thread
    ///     to be freed up to handle other work while waiting for the database operation to complete.
    ///     What happens here? First we save changes to the DbContext (writes to the database),
    ///     then we commit the transaction (makes those changes permanent and visible to others).
    /// </summary>
    public async Task Commit()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LocalVaultScope));
        await transaction.CommitAsync();
    }

    /// <summary>
    ///     Rolls back all changes made within this scope, discarding them.
    ///     How does this work? The database transaction is rolled back, which undoes all
    ///     database changes made since the transaction began. We also clear EF Core's
    ///     change tracker to discard any in-memory changes.
    /// </summary>
    public async Task Rollback()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LocalVaultScope));
        await transaction.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
