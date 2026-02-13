using Microsoft.EntityFrameworkCore;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Entity Framework DbContext for the local vault database.
///     This manages the connection to the SQLite database and provides access to vault entities.
/// </summary>
public class LocalVaultDbContext(DbContextOptions<LocalVaultDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Accounts table mapping.
    /// </summary>
    public DbSet<AccountEntity> Accounts { get; set; } = null!;

    /// <summary>
    /// Ledger entries table mapping.
    /// </summary>
    public DbSet<LedgerEntryEntity> LedgerEntries { get; set; } = null!;

    /// <summary>
    /// Metadata marker table mapping used to identify Zylance vaults.
    /// </summary>
    public DbSet<ZylanceMetadataEntity> ZylanceMetadata { get; set; } = null!;
}
