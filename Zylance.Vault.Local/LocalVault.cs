using Microsoft.EntityFrameworkCore;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;
using Zylance.Vault.Local.Context;

namespace Zylance.Vault.Local;

/// <summary>
///     Local vault implementation using SQLite database through Entity Framework Core.
/// </summary>
public class LocalVault(LocalVaultDbContext dbContext) : IVault
{
    private readonly LocalVaultDbContext _dbContext = dbContext;

    /// <summary>
    ///     Creates a LocalVault instance from a file path.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file</param>
    /// <returns>A new LocalVault instance</returns>
    public static LocalVault FromFile(string filePath)
    {
        // Configure Entity Framework Core to use SQLite with the provided file path
        var optionsBuilder = new DbContextOptionsBuilder<LocalVaultDbContext>();
        optionsBuilder.UseSqlite($"Data Source={filePath}");

        // Create the DbContext with the configured options
        var dbContext = new LocalVaultDbContext(optionsBuilder.Options);

        // Ensure the database schema is created if it doesn't exist
        // This creates tables based on your DbContext model
        dbContext.Database.EnsureCreated();

        return new LocalVault(dbContext);
    }
}
