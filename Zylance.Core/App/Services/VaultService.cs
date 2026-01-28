using Zylance.Contract.Api.Vault;
using Zylance.Core.Lib.Interfaces;

namespace Zylance.Core.App.Services;

public class VaultService(IVaultProvider vaultProvider)
{
    private readonly Dictionary<string, IVault> _openedVaults = new();

    public VaultRef OpenVault()
    {
        var vault = vaultProvider.OpenVault();
        var vaultId = Guid.NewGuid().ToString();
        _openedVaults[vaultId] = vault;
        return new VaultRef { Id = vaultId };
    }

    public VaultRef CreateVault()
    {
        var vault = vaultProvider.CreateVault();
        var vaultId = Guid.NewGuid().ToString();
        _openedVaults[vaultId] = vault;
        return new VaultRef { Id = vaultId };
    }

    public IVault GetVault(VaultRef vaultRef)
    {
        return _openedVaults.TryGetValue(vaultRef.Id, out var vault)
            ? vault
            : throw new KeyNotFoundException($"Vault with ID '{vaultRef.Id}' is not open");
    }
}
