using Zylance.Core.Lib.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Elements;

public record OfxStatus
{
    public required short Code { get; init; }
    public string? Severity { get; init; }
    public string? Message { get; init; }

    internal static OfxStatus FromRaw(OfxRawElement getChildElement)
    {
        throw new NotImplementedException();
    }
}
