namespace Zylance.Core.Lib.Vault;

public interface IVaultProvider
{
    public IVault OpenVault();
    public IVault CreateVault();
}
