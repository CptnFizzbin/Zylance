using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zylance.Vault.Local.Context;

public class LocalVaultContextFactory : IDesignTimeDbContextFactory<LocalVaultDbContext>
{
    public LocalVaultDbContext CreateDbContext(string[] args)
    {
        var filePath = args.Length > 0 ? args[0] : "localvault.zlv.sqlite";
        return CreateDbContextFromFile(filePath);
    }

    public static LocalVaultDbContext CreateDbContextFromFile(string filePath = "localvault.zlv.sqlite")
    {
        var connectionString = $"Data Source={filePath}";
        return CreateDbContext(connectionString);
    }

    private static LocalVaultDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocalVaultDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        var dbContext = new LocalVaultDbContext(optionsBuilder.Options);
        return dbContext;
    }
}
