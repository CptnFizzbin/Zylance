using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1;

namespace Zylance.Core.Importers.Ofx;

/// <summary>
///     Top-level OFX parser that detects the OFX version and delegates to the appropriate version-specific parser.
/// </summary>
public class OfxParser
{
    /// <summary>
    ///     Parses an OFX file and returns a list of statements.
    ///     Automatically detects the OFX version (V1/SGML or V2/XML) and uses the appropriate parser.
    /// </summary>
    /// <param name="content">StreamReader containing the OFX file content</param>
    /// <returns>List of OFX statements parsed from the file</returns>
    public List<OfxStatement> Parse(StreamReader content)
    {
        // TODO: Implement version detection logic
        return OfxV1Parser.Parse(content);
    }
}
