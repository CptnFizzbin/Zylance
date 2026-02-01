using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zylance.Vault.Local.Context;

public class LocalVaultContextFactory : IDesignTimeDbContextFactory<LocalVaultDbContext>
{
    public LocalVaultDbContext CreateDbContext(string[] args)
    {
        return CreateDbContextFromFile(args[0]);
    }

    public static LocalVaultDbContext CreateDbContextFromFile(string filePath = "localvault.zlv")
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
