using Zylance.Contract.Messages.Vault;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;

namespace Zylance.Core.Services;

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
