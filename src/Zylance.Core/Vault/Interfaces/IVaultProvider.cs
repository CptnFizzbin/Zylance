using Zylance.Contract.Models.Vault;

namespace Zylance.Core.Vault.Interfaces;

/// <summary>
///     Provider abstraction responsible for opening or creating vault instances
///     (platform-specific).
/// </summary>
public interface IVaultProvider
{
    /// <summary>
    ///     Opens an existing vault and returns an IVault implementation.
    /// </summary>
    public Task<IVault> OpenVault();

    /// <summary>
    ///     Creates a new vault instance and returns an IVault implementation.
    /// </summary>
    public Task<IVault> CreateVault();

    /// <summary>
    ///     Lists recently accessed vaults, to allow for quick access.
    ///     This is optional and may return an empty list if not supported.
    /// </summary>
    /// <returns>A list of recently opened vaults</returns>
    public Task<List<VaultRef>> RecentVaults()
    {
        return Task.FromResult(new List<VaultRef>());
    }
}
