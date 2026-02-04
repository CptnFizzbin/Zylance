using System.Text;
using Zylance.Core.Lib.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Tests.Importers.Ofx.V1;

public static class ParserTestHelper
{
    /// <summary>
    /// Loads an OFX fixture file from the V1 fixtures directory
    /// </summary>
    public static StreamReader LoadFixture(string filename)
    {
        var filePath = Path.Combine("Importers", "Fixtures", "Ofx", "V1", filename);
        return new TrimmingStreamReader(File.OpenRead(filePath));
    }

    /// <summary>
    /// Converts a string containing OFX content to a StreamReader.
    /// Returns a TrimmingStreamReader that automatically trims whitespace from lines.
    /// </summary>
    public static StreamReader StringToStreamReader(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new TrimmingStreamReader(stream);
    }
}
