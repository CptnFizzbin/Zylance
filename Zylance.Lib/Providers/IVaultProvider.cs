using Zylance.Lib.Models;

namespace Zylance.Lib.Providers;

public interface IVaultProvider
{
    public IVault OpenVault();
    public IVault CreateVault();
}
