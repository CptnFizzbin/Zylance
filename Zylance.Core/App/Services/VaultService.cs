using Zylance.Contract.Models.Vault;
using Zylance.Core.Lib.Vault;

namespace Zylance.Core.App.Services;

public class VaultService(
    IVaultProvider vaultProvider,
    VaultContext vaultContext,
    BackgroundTaskService backgroundTaskService
)
{
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

    public void CloseVault()
    {
        vaultContext.ActiveVault = null;
    }

    public VaultRef? GetActiveVaultRef()
    {
        return vaultContext.ActiveVault?.ToRef();
    }
}
