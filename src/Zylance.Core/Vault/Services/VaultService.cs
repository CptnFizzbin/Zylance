using Serilog;
using Zylance.Contract.Models.Vault;
using Zylance.Core.Logging;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Interfaces;

namespace Zylance.Core.Vault.Services;

/// <summary>
///     Application-level service that coordinates opening, creating, and closing
///     the active vault.
///     Exposes helpers used by the UI to manage the current vault lifecycle.
/// </summary>
public class VaultService(
    IVaultProvider vaultProvider,
    VaultContext vaultContext,
    BackgroundTaskService backgroundTaskService
)
{
    private static readonly ILogger Log = ZyLogger.ForContext<VaultService>();

    /// <summary>
    ///     Opens the active vault and returns its reference.
    /// </summary>
    public async Task<VaultRef> OpenVault()
    {
        return await backgroundTaskService.WithProgress(
            "Opening vault...",
            async _ =>
            {
                var vault = await vaultProvider.OpenVault();
                var vaultId = Guid.NewGuid().ToString();

                vaultContext.ActiveVault = vault;

                return new VaultRef { Id = vaultId };
            }
        );
    }

    /// <summary>
    ///     Creates a new vault, sets it active, and returns its reference.
    /// </summary>
    public async Task<VaultRef> CreateVault()
    {
        return await backgroundTaskService.WithProgress(
            "Creating new vault...",
            async _ =>
            {
                var vault = await vaultProvider.CreateVault();

                var vaultId = Guid.NewGuid().ToString();
                vaultContext.ActiveVault = vault;

                return new VaultRef { Id = vaultId };
            }
        );
    }

    /// <summary>
    ///     Closes the currently active vault.
    /// </summary>
    public void CloseVault()
    {
        vaultContext.ActiveVault = null;
    }

    /// <summary>
    ///     Returns a reference to the currently active vault, or null if none is
    ///     active.
    /// </summary>
    public VaultRef? GetActiveVaultRef()
    {
        return vaultContext.ActiveVault?.ToRef();
    }
}
