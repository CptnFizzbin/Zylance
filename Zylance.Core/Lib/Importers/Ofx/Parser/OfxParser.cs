using Zylance.Core.Lib.Importers.Ofx.Elements;
using Zylance.Core.Lib.Importers.Ofx.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Parser;

public class OfxParser
{
    public OfxFile Parse(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        return OfxFile.FromRaw(rawFile);
    }
}
