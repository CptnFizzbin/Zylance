using Microsoft.EntityFrameworkCore;
using Zylance.Lib.Converters;
using Zylance.Lib.Entities;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Entity Framework DbContext for the local vault database.
///     This manages the connection to the SQLite database and provides access to vault entities.
/// </summary>
public class LocalVaultDbContext(DbContextOptions<LocalVaultDbContext> options) : DbContext(options)
{
    public DbSet<TransactionEntity> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.Property(e => e.Debit)
                .HasConversion(MonetaryValueConverters.NullableMonetaryValueConverter);

            entity.Property(e => e.Credit)
                .HasConversion(MonetaryValueConverters.NullableMonetaryValueConverter);
        });
    }
}
