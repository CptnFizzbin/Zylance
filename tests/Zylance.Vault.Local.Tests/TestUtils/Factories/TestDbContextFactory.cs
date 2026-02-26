using Microsoft.EntityFrameworkCore;
using Zylance.Vault.Local.Context;

namespace Zylance.Vault.Local.Tests.TestUtils.Factories;

/// <summary>
///     Factory for creating test database contexts with in-memory database.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    ///     Creates a new LocalVaultDbContext with an in-memory database.
    ///     Each call creates a unique database to ensure test isolation.
    /// </summary>
    public static LocalVaultDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LocalVaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new LocalVaultDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
