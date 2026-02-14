using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Vault.Interfaces;
using Zylance.Vault.Local;

namespace Zylance.Desktop.Providers;

/// <summary>
///     Desktop implementation of <see cref="IVaultProvider" /> that opens and
///     creates local vault files.
/// </summary>
/// <param name="fileSystem">Local file provider used to select and manage files.</param>
public class DesktopVaultProvider(ILocalFileProvider fileSystem) : IVaultProvider
{
    /// <summary>
    ///     Opens an existing vault selected by the user.
    /// </summary>
    public async Task<IVault> OpenVault()
    {
        var filters = new List<(string Name, string[] Extensions)> { ("Zylance Vault", [".zlv"]) };

        var fileRef = await fileSystem.SelectFile("Open Vault", filters.ToArray(), false);
        var path = await fileSystem.GetFilePath(fileRef);

        if (!path.EndsWith(".zlv", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Selected file is not a valid Zylance Vault (.zlv) file.");

        return await LocalVault.FromFile(path);
    }

    /// <summary>
    ///     Creates a new vault file at a user-selected location.
    /// </summary>
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
