using Serilog;
using Zylance.Core.Logging;
using Zylance.Core.Vault.Managers;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Entities;

namespace Zylance.Vault.Local.Managers;

/// <summary>
///     Local implementation of IMetadataManager using the _zylance_ marker table.
/// </summary>
public class LocalMetadataManager(LocalVaultDbContext dbContext) : IMetadataManager
{
    private static readonly ILogger Log = ZyLogger.ForContext<LocalMetadataManager>();

    /// <summary>
    ///     Gets a metadata value by key from the marker table.
    /// </summary>
    /// <param name="key">Metadata key to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.ZylanceMetadata.FindAsync([key], cancellationToken);
        return entity?.Value;
    }

    /// <summary>
    ///     Sets a metadata key/value pair in the marker table.
    /// </summary>
    /// <param name="key">Metadata key to set.</param>
    /// <param name="value">Value to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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
