using Microsoft.EntityFrameworkCore;
using Zylance.Core.Lib.Vault.Managers;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Local implementation of IMetadataManager using the _zylance_ marker table.
/// </summary>
public class LocalMetadataManager(LocalVaultDbContext dbContext) : IMetadataManager
{
    public async Task<string?> GetAsync(string key)
    {
        var entity = await dbContext.ZylanceMetadata.FindAsync(key);
        return entity?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var entity = await dbContext.ZylanceMetadata.FindAsync(key);

        if (entity is not null)
        {
            // Update existing entry
            entity.Value = value;
        }
        else
        {
            // Create new entry
            // Note: For concurrent scenarios, consider using ExecuteUpdateAsync
            // or handling DbUpdateException for duplicate key violations
            dbContext.ZylanceMetadata.Add(new ZylanceMetadataEntity { Key = key, Value = value });
        }

        await dbContext.SaveChangesAsync();
    }
}
