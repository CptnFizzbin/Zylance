using Zylance.Contract.Models.File;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Importers.Ofx;

public class OfxImporter(FileService fileService) : IImporter
{
    // FileService will be used when ImportAsync is implemented
    private readonly FileService _fileService = fileService;

    public IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; } =
        [("Open Financial Exchange", [".ofx"]), ("Quicken Financial Exchange", [".qfx"])];

    public async Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
