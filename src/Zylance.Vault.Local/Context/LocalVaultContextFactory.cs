using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zylance.Vault.Local.Context;

/// <summary>
///     Factory used to create LocalVaultDbContext instances for design-time and
///     runtime scenarios.
/// </summary>
public class LocalVaultContextFactory : IDesignTimeDbContextFactory<LocalVaultDbContext>
{
    /// <summary>
    ///     Creates a new DbContext using the provided command-line args (first arg is
    ///     file path).
    ///     Used by EntityFramework generator utilities
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    public LocalVaultDbContext CreateDbContext(string[] args)
    {
        var filePath = args.Length > 0 ? args[0] : "localvault.zlv.sqlite";
        return CreateDbContextFromFile(filePath);
    }

    /// <summary>
    ///     Creates a DbContext configured to use the given SQLite file.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file.</param>
    public static LocalVaultDbContext CreateDbContextFromFile(string filePath = "localvault.zlv.sqlite")
    {
        var connectionString = $"Data Source={filePath}";
        return CreateDbContext(connectionString);
    }

    /// <summary>
    ///     Internal helper to create a DbContext from a connection string.
    /// </summary>
    private static LocalVaultDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocalVaultDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        var dbContext = new LocalVaultDbContext(optionsBuilder.Options);
        return dbContext;
    }
}
