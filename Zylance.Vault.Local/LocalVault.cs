using Microsoft.EntityFrameworkCore;
using Zylance.Core.Lib.Vault;
using Zylance.Core.Lib.Vault.Managers;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Managers;

namespace Zylance.Vault.Local;

/// <summary>
///     Local vault implementation using SQLite database through Entity Framework Core.
/// </summary>
public class LocalVault(LocalVaultDbContext dbContext) : IVault
{
    /// <summary>
    ///     Gets the account manager for managing account records.
    /// </summary>
    public IAccountManager Accounts { get; } = new LocalAccountManager(dbContext);

    /// <summary>
    ///     Gets the ledger manager for managing ledger entry records.
    /// </summary>
    public ILedgerManager Ledgers { get; } = new LocalLedgerManager(dbContext);

    public Guid VaultId { get; } = Guid.CreateVersion7();
    public bool Unlocked => true; // TODO: Implement lock state management

    /// <summary>
    ///     Creates a new transactional scope for vault operations.
    ///     Why reuse the connection? Creating new database connections is expensive and can
    ///     exhaust the connection pool. SQLite handles transaction isolation at the connection
    ///     level, so we can safely reuse the same connection with nested transactions.
    /// </summary>
    public IVaultScope CreateScope()
    {
        // Start a new transaction on the existing connection
        // Note: EF Core supports nested transactions via savepoints in SQLite
        var transaction = dbContext.Database.BeginTransaction();

        return new LocalVaultScope(this, transaction);
    }

    public async Task WithScope(Func<IVaultScope, Task> action)
    {
        await using var scope = CreateScope();
        try
        {
            await action(scope);
            await scope.Commit();
        }
        catch
        {
            await scope.Rollback();
            throw;
        }
    }

    /// <summary>
    ///     Creates a LocalVault instance from a file path.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file</param>
    /// <returns>A new LocalVault instance</returns>
    public async static Task<LocalVault> FromFile(string filePath)
    {
        var dbContext = LocalVaultContextFactory.CreateDbContextFromFile(filePath);
        await dbContext.Database.MigrateAsync();
        return new LocalVault(dbContext);
    }
}
