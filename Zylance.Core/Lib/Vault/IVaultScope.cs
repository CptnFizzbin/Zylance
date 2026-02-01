namespace Zylance.Core.Lib.Vault;

public interface IVaultScope : IAsyncDisposable
{
    public IVault Vault { get; }
    public Task Commit();
    public Task Rollback();
}
