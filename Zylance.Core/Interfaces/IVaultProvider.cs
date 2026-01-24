using Zylance.Core.Models;

namespace Zylance.Core.Providers;

public interface IVaultProvider
{
    public IVault OpenVault();
    public IVault CreateVault();
}
