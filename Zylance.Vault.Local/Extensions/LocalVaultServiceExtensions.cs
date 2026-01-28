using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zylance.Vault.Local.Context;

namespace Zylance.Vault.Local.Extensions;

/// <summary>
///     Extension methods for configuring the Local Vault with Entity Framework SQLite.
/// </summary>
public static class LocalVaultServiceExtensions
{
    /// <summary>
    ///     Adds LocalVault services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="databasePath">Path to the SQLite database file (e.g., "vault.db")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLocalVault(
        this IServiceCollection services,
        string databasePath
    )
    {
        // Register the DbContext with SQLite
        services.AddDbContext<LocalVaultDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        // Register the LocalVault implementation
        services.AddScoped<LocalVault>();

        return services;
    }

    /// <summary>
    ///     Ensures the database is created and applies any pending migrations.
    ///     Call this during application startup.
    /// </summary>
    public static void EnsureVaultDatabaseCreated(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LocalVaultDbContext>();

        // This creates the database if it doesn't exist
        // For production, consider using Migrations instead
        dbContext.Database.EnsureCreated();
    }
}
