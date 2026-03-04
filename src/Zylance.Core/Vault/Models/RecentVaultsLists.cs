using Zylance.Contract.Models.Vault;
using Zylance.Core.Vault.Interfaces;

namespace Zylance.Core.Vault.Models;

public class RecentVaultsList : Dictionary<string, List<RecentVaultRef>>
{
    public void AddVault(IVault vault)
    {
        var vaultRef = new RecentVaultRef { Name = vault.Name, Location = vault.Location };

        var recentVaults = this.GetValueOrDefault(vault.ProviderId) ?? [];
        recentVaults.RemoveAll(v => v.Location == vaultRef.Location);
        recentVaults.Insert(0, vaultRef);
        this[vault.ProviderId] = recentVaults;
    }
}
