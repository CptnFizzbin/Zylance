using JetBrains.Annotations;
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
    [UsedImplicitly]
    private readonly LocalVaultDbContext _dbContext = dbContext;

    private IAccountManager? _accountManager;
    private ILedgerManager? _ledgerManager;

    /// <summary>
    ///     Gets the account manager for managing account records.
    ///     Why use lazy initialization? The managers are created on-demand rather than in the
    ///     constructor, which reduces initialization cost if they're never used. The backing
    ///     field pattern ensures we only create the manager instance once.
    /// </summary>
    public IAccountManager Accounts => _accountManager ??= new LocalAccountManager(_dbContext);

    /// <summary>
    ///     Gets the ledger manager for managing ledger entry records.
    /// </summary>
    public ILedgerManager Ledgers => _ledgerManager ??= new LocalLedgerManager(_dbContext);

    /// <summary>
    ///     Creates a new transactional scope for vault operations.
    ///     Why reuse the connection? Creating new database connections is expensive and can
    ///     exhaust the connection pool. SQLite handles transaction isolation at the connection
    ///     level, so we can safely reuse the same connection with nested transactions.
    ///     Why start a transaction? EF Core's BeginTransaction() starts an actual database
    ///     transaction that ensures ACID properties - all changes in the scope either commit
    ///     together or roll back together if any operation fails.
    /// </summary>
    public IVaultScope CreateScope()
    {
        // Start a new transaction on the existing connection
        // Note: EF Core supports nested transactions via savepoints in SQLite
        var transaction = _dbContext.Database.BeginTransaction();

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
