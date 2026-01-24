using Zylance.Core.Models;

namespace Zylance.Core.Interfaces;

public interface IVaultProvider
{
    public IVault OpenVault();
    public IVault CreateVault();
}
