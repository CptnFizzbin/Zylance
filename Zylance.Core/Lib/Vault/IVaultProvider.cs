namespace Zylance.Core.Lib.Vault;

public interface IVaultProvider
{
    public Task<IVault> OpenVault();
    public Task<IVault> CreateVault();
}
