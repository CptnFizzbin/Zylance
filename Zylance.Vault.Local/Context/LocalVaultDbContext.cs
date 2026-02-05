using Microsoft.EntityFrameworkCore;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Entity Framework DbContext for the local vault database.
///     This manages the connection to the SQLite database and provides access to vault entities.
/// </summary>
public class LocalVaultDbContext(DbContextOptions<LocalVaultDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<LedgerEntryEntity> LedgerEntries { get; set; }
    public DbSet<ZylanceMetadataEntity> ZylanceMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the _zylance_ marker table
        modelBuilder.Entity<ZylanceMetadataEntity>(entity =>
        {
            entity.ToTable("_zylance_");
            entity.HasKey(e => e.Key);
        });
    }
}
