using Zylance.Contract.Models.Vault;
using Zylance.Core.Lib.Vault;

namespace Zylance.Core.App.Services;

public class VaultService(IVaultProvider vaultProvider, VaultContext vaultContext)
{
    public async Task<VaultRef> OpenVault()
    {
        var vault = await vaultProvider.OpenVault();
        var vaultId = Guid.NewGuid().ToString();
        vaultContext.ActiveVault = vault;
        return new VaultRef { Id = vaultId };
    }

    public async Task<VaultRef> CreateVault()
    {
        var vault = await vaultProvider.CreateVault();
        var vaultId = Guid.NewGuid().ToString();
        vaultContext.ActiveVault = vault;
        return new VaultRef { Id = vaultId };
    }

    public void CloseVault()
    {
        vaultContext.ActiveVault = null;
    }

    public VaultRef? GetStatus()
    {
        return vaultContext.ActiveVault?.ToRef();
    }
}
