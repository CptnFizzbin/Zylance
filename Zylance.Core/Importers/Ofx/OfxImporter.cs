using Zylance.Contract.Models.File;
using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Importers.Ofx;

public class OfxImporter : IImporter
{
    public IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; } =
    [("Open Financial Exchange", [".ofx"]), ("Quicken Financial Exchange", [".qfx"])];

    public Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
