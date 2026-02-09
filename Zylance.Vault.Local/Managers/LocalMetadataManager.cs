using Zylance.Core.Lib.Vault.Managers;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Local implementation of IMetadataManager using the _zylance_ marker table.
/// </summary>
public class LocalMetadataManager(LocalVaultDbContext dbContext) : IMetadataManager
{
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ZylanceMetadata.FindAsync([key], cancellationToken);
        return entity?.Value;
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ZylanceMetadata.FindAsync([key], cancellationToken);

        if (entity is not null)
            entity.Value = value;
        else
            dbContext.ZylanceMetadata.Add(new ZylanceMetadataEntity { Key = key, Value = value });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
