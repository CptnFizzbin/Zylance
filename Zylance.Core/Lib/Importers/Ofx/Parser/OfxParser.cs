using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.V1.Parser;

namespace Zylance.Core.Lib.Importers.Ofx.Parser;

/// <summary>
/// Top-level OFX parser that detects the OFX version and delegates to the appropriate version-specific parser.
/// </summary>
public class OfxParser
{
    /// <summary>
    /// Parses an OFX file and returns a list of statements.
    /// Automatically detects the OFX version (V1/SGML or V2/XML) and uses the appropriate parser.
    /// </summary>
    /// <param name="content">StreamReader containing the OFX file content</param>
    /// <returns>List of OFX statements parsed from the file</returns>
    public async Task<List<OfxStatement>> ParseAsync(StreamReader content)
    {
        // TODO: Implement version detection logic
        // For now, always use V1 parser
        var v1Parser = new OfxV1Parser();
        return await v1Parser.ParseAsync(content);
    }
}
