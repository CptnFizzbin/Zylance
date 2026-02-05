namespace Zylance.Core.Lib.Vault.Managers;

/// <summary>
///     Manager for vault metadata operations.
///     Provides access to key-value metadata stored in the vault.
/// </summary>
public interface IMetadataManager
{
    /// <summary>
    ///     Retrieves a metadata value by key.
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <returns>The metadata value, or null if the key does not exist</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    ///     Sets a metadata value for a key.
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <param name="value">The metadata value</param>
    Task SetAsync(string key, string value);
}
