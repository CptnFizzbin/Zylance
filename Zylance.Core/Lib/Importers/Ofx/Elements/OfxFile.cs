using Zylance.Core.Lib.Importers.Ofx.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Elements;

public record OfxFile
{
    public Dictionary<string, string> Headers { get; init; } = [];
    public required OfxRoot Root { get; init; }

    internal static OfxFile FromRaw(OfxRawFile rawFile)
    {
        return new OfxFile
        {
            Headers = rawFile.Headers.ToDictionary(k => k.Name, v => v.Value),
            Root = OfxRoot.FromRaw(rawFile.Root),
        };
    }
}
