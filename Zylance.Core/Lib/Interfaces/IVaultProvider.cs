namespace Zylance.Core.Lib.Interfaces;

public interface IVaultProvider
{
    public IVault OpenVault();
    public IVault CreateVault();
}
