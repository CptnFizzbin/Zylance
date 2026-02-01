using Zylance.Core.Lib.Interfaces;
using Zylance.Core.Lib.Vault;
using Zylance.Vault.Local;

namespace Zylance.Desktop;

public class DesktopVaultProvider(ILocalFileProvider fileSystem) : IVaultProvider
{
    public async Task<IVault> OpenVault()
    {
        var filters = new List<(string Name, string[] Extensions)> { ("Zylance Vault", [".zlv"]) };

        var fileRef = fileSystem.SelectFile("Open Vault", filters.ToArray(), false);
        var path = fileSystem.GetFilePath(fileRef);

        return await LocalVault.FromFile(path);
    }

    public async Task<IVault> CreateVault()
    {
        var filters = new List<(string Name, string[] Extensions)> { ("Zylance Vault", [".zlv"]) };

        var fileRef = fileSystem.CreateFile("Create Vault", "vault.zlv", filters.ToArray());
        var path = fileSystem.GetFilePath(fileRef);

        return await LocalVault.FromFile(path);
    }
}
