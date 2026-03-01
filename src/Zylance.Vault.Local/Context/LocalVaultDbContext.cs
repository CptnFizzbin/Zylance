using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Entity Framework DbContext for the local vault database.
///     This manages the connection to the SQLite database and provides access to
///     vault entities.
/// </summary>
public class LocalVaultDbContext(DbContextOptions<LocalVaultDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Accounts table mapping.
    /// </summary>
    public DbSet<AccountEntity> Accounts { get; set; } = null!;

    /// <summary>
    ///     Ledger entries table mapping.
    /// </summary>
    public DbSet<LedgerEntryEntity> LedgerEntries { get; set; } = null!;

    /// <summary>
    ///     Metadata marker table mapping used to identify Zylance vaults.
    /// </summary>
    public DbSet<ZylanceMetadataEntity> ZylanceMetadata { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        if (Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite")
            return;

        // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
        // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
        // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
        // use the DateTimeOffsetToBinaryConverter
        // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
        // This only supports millisecond precision, but should be sufficient for most use cases.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var properties = entityType
                .ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
            foreach (var property in properties)
                builder
                    .Entity(entityType.Name)
                    .Property(property.Name)
                    .HasConversion(new DateTimeOffsetToBinaryConverter());
        }
    }
}
