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
}
