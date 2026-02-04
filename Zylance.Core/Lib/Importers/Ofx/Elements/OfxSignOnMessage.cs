using Zylance.Core.Lib.Importers.Ofx.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Elements;

public class OfxSignOnMessage
{
    public required OfxStatus Status { get; init; }

    internal static OfxSignOnMessage FromRaw(OfxRawElement getChildElement)
    {
        throw new NotImplementedException();
    }
}
