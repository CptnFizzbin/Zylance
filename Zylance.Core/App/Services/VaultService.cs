using Zylance.Contract.Models.Vault;
using Zylance.Core.Lib.Vault;

namespace Zylance.Core.App.Services;

public class VaultException(string message) : Exception(message)
{
    public static VaultException NoActiveVault()
    {
        return new VaultException("No active vault. Please open or create a vault before performing operations.");
    }
}

public class VaultService(IVaultProvider vaultProvider)
{
    public IVault? ActiveVault { get; private set; }

    public async Task<VaultRef> OpenVault()
    {
        var vault = await vaultProvider.OpenVault();
        var vaultId = Guid.NewGuid().ToString();
        ActiveVault = vault;
        return new VaultRef { Id = vaultId };
    }

    public async Task<VaultRef> CreateVault()
    {
        var vault = await vaultProvider.CreateVault();
        var vaultId = Guid.NewGuid().ToString();
        ActiveVault = vault;
        return new VaultRef { Id = vaultId };
    }
}
