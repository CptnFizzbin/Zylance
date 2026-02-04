using Zylance.Contract.Models.File;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Importers;

public class OfxImporter(FileService fileService) : IImporter
{
    public IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; } =
        [("Open Financial Exchange", [".ofx"]), ("Quicken Financial Exchange", [".qfx"])];

    public async Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
