using Zylance.Core.Lib;
using Zylance.Core.Lib.Vault;
using Zylance.Vault.Local;

namespace Zylance.Desktop;

public class DesktopVaultProvider(ILocalFileProvider fileSystem) : IVaultProvider
{
    public async Task<IVault> OpenVault()
    {
        var filters = new List<(string Name, string[] Extensions)> { ("Zylance Vault", [".zlv"]) };

        var fileRef = await fileSystem.SelectFile("Open Vault", filters.ToArray(), false);
        var path = await fileSystem.GetFilePath(fileRef);

        if (!path.EndsWith(".zlv", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Selected file is not a valid Zylance Vault (.zlv) file.");

        return await LocalVault.FromFile(path);
    }

    public async Task<IVault> CreateVault()
    {
        var filters = new List<(string Name, string[] Extensions)> { ("Zylance Vault", [".zlv"]) };

        var fileRef = await fileSystem.CreateFile(
            "Create Vault",
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            filters.ToArray()
        );
        var path = await fileSystem.GetFilePath(fileRef);

        if (!path.EndsWith(".zlv", StringComparison.OrdinalIgnoreCase))
            path += ".zlv";

        return await LocalVault.FromFile(path);
    }
}
