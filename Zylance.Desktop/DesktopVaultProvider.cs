using Zylance.Core.Interfaces;
using Zylance.Vault.Local;

namespace Zylance.Desktop;

public class DesktopVaultProvider(ILocalFileProvider fileSystem) : IVaultProvider
{
    public IVault OpenVault()
    {
        var filters = new List<(string Name, string[] Extensions)>
        {
            ("Zylance Vault", [".zlv"]),
        };

        var fileRef = fileSystem.SelectFile("Open Vault", filters.ToArray(), false);
        var path = fileSystem.GetFilePath(fileRef);

        return LocalVault.FromFile(path);
    }

    public IVault CreateVault()
    {
        var filters = new List<(string Name, string[] Extensions)>
        {
            ("Zylance Vault", [".zlv"]),
        };

        var fileRef = fileSystem.CreateFile("Create Vault", "vault.zlv", filters.ToArray());
        var path = fileSystem.GetFilePath(fileRef);

        return LocalVault.FromFile(path);
    }
}
