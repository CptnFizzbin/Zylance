using Microsoft.EntityFrameworkCore;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Entity Framework DbContext for the local vault database.
///     This manages the connection to the SQLite database and provides access to vault entities.
/// </summary>
public class LocalVaultDbContext(DbContextOptions<LocalVaultDbContext> options) : DbContext(options)
{
    public DbSet<TransactionEntity> Transactions { get; set; } = null!;
}
