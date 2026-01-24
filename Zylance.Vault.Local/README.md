# Zylance.Vault.Local

Local vault implementation using SQLite database through Entity Framework Core.

## Setup

### 1. Add the Package References

The project already includes:
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite database provider
- `Microsoft.EntityFrameworkCore.Design` - Design-time tools for migrations
- `Microsoft.Extensions.DependencyInjection.Abstractions` - For DI support

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Register Services

In your application's startup/configuration code:

```csharp
using Zylance.Vault.Local;

// Add to your service collection
services.AddLocalVault("path/to/vault.db");

// Later, ensure the database is created (call this during app startup)
serviceProvider.EnsureVaultDatabaseCreated();
```

### 4. Use the Vault

```csharp
public class MyService
{
    private readonly LocalVault _vault;

    public MyService(LocalVault vault)
    {
        _vault = vault;
    }

    // Your vault operations here
}
```

## Database Structure

Currently, the `LocalVaultDbContext` is set up with no entities. You'll need to:

1. **Define your entity models** - Create classes representing your data
2. **Add DbSet properties** to `LocalVaultDbContext`
3. **Configure relationships** in `OnModelCreating` method
4. **Implement IVault methods** in `LocalVault.cs`

### Example Entity

```csharp
public class VaultEntry
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Adding to DbContext

```csharp
public DbSet<VaultEntry> VaultEntries { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<VaultEntry>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Key).IsUnique();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    });
}
```

## Migrations (Optional but Recommended for Production)

For production use, consider using EF Core migrations instead of `EnsureCreated()`:

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project Zylance.Vault.Local

# Apply migrations
dotnet ef database update --project Zylance.Vault.Local
```

## Notes

- **EnsureCreated() vs Migrations**: `EnsureCreated()` is quick for development but doesn't support schema updates. For production, use migrations.
- **Connection Strings**: Currently using a simple file path. You can customize this in `LocalVaultServiceExtensions.cs`.
- **Async Operations**: Consider making your IVault methods async for better performance with EF Core.
