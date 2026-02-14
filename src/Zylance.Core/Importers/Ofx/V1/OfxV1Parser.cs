using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1;

/// <summary>
///     Parses OFX V1 (SGML) format files and returns structured statement data.
/// </summary>
public class OfxV1Parser
{
    /// <summary>
    ///     Parses an OFX V1 file and returns a list of statements.
    ///     Each statement contains an account, balance information, and transactions.
    /// </summary>
    /// <param name="content">StreamReader containing the OFX file content</param>
    /// <returns>List of OFX statements parsed from the file</returns>
    public static List<OfxStatement> Parse(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        return ExtractStatements(rawFile.Root);
    }

    private static List<OfxStatement> ExtractStatements(OfxRawElement element)
    {
        var statements = new List<OfxStatement>();

        switch (element.Name)
        {
            case OfxTagNames.StatementTransactionsRes:
            case OfxTagNames.CreditCardStatementTransactionsRes:
            {
                statements.Add(OfxV1Statement.From(element));
                break;
            }
        }

        foreach (var child in element.Children)
            statements.AddRange(ExtractStatements(child));

        return statements;
    }
}
