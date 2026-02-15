using Microsoft.EntityFrameworkCore;
using Serilog;
using Zylance.Core.Logging;
using Zylance.Core.Vault.Interfaces;
using Zylance.Core.Vault.Managers;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Managers;

namespace Zylance.Vault.Local;

/// <summary>
///     Local vault implementation using SQLite database through Entity Framework
///     Core.
/// </summary>
public class LocalVault(LocalVaultDbContext dbContext) : IVault, IAsyncDisposable
{
    private static readonly ILogger Log = ZyLogger.CreateLogger<LocalVault>();

    /// <summary>
    ///     Dispose the local vault and its underlying database context.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }

    /// <summary>
    ///     Gets the account manager for managing account records.
    /// </summary>
    public IAccountManager Accounts { get; } = new LocalAccountManager(dbContext);

    /// <summary>
    ///     Gets the ledger manager for managing ledger entry records.
    /// </summary>
    public ILedgerManager Ledgers { get; } = new LocalLedgerManager(dbContext);

    /// <summary>
    ///     Gets the metadata manager for managing vault metadata.
    /// </summary>
    public IMetadataManager Metadata { get; } = new LocalMetadataManager(dbContext);

    /// <summary>
    ///     Unique identifier for this vault instance.
    /// </summary>
    public Guid VaultId { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     Indicates whether the vault is locked for modifications.
    /// </summary>
    public bool Locked => false; // TODO: Implement lock state management

    /// <summary>
    ///     Creates a new transactional scope for vault operations.
    ///     Why reuse the connection? Creating new database connections is expensive
    ///     and can
    ///     exhaust the connection pool. SQLite handles transaction isolation at the
    ///     connection
    ///     level, so we can safely reuse the same connection with nested transactions.
    /// </summary>
    public IVaultScope CreateScope()
    {
        // Start a new transaction on the existing connection
        // Note: EF Core supports nested transactions via savepoints in SQLite
        var transaction = dbContext.Database.BeginTransaction();

        return new LocalVaultScope(this, transaction);
    }

    /// <summary>
    ///     Creates a LocalVault instance from a file path.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new LocalVault instance</returns>
    /// <exception cref="NonZylanceDatabaseException">
    ///     Thrown when the database exists but does not contain the _zylance_ marker
    ///     table
    /// </exception>
    public static async Task<LocalVault> FromFile(string filePath, CancellationToken cancellationToken = default)
    {
        var dbContext = LocalVaultContextFactory.CreateDbContextFromFile(filePath);

        try
        {
            var fileExists = File.Exists(filePath);

            if (fileExists)
                await AssertZylanceVault(dbContext, filePath, cancellationToken);

            // Use EnsureCreated in tests and runtime paths to avoid migrations pending checks
            // and create schema directly from the current model when the database is new.
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            return new LocalVault(dbContext);
        }
        catch
        {
            await dbContext.DisposeAsync();
            throw;
        }
    }

    private static async Task AssertZylanceVault(
        LocalVaultDbContext dbContext,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
            throw NonZylanceDatabaseException.InvalidFile(filePath);

        var connection = dbContext.Database.GetDbConnection();
        try
        {
            await connection.OpenAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw NonZylanceDatabaseException.InvalidFile(filePath, exception);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='_zylance_'";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            var hasMarkerTable = result is not null && Convert.ToInt32(result) > 0;

            if (hasMarkerTable)
                return;

            throw new NonZylanceDatabaseException(filePath, "The required '_zylance_' marker table was not found.");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
