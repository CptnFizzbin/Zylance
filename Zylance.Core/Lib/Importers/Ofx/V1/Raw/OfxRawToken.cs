using System.Text.RegularExpressions;
using Zylance.Core.Lib.Importers.Ofx.V1.Parser;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Raw;

internal partial record OfxRawToken
{
    public required string Name { get; init; }
    public required string Value { get; init; }

    public DateTimeOffset? DateTimeValue => DateTimeOffsetParser.TryParse(Value, out var dto)
        ? dto
        : null;

    public decimal? DecimalValue => decimal.TryParse(Value, out var parsed)
        ? parsed
        : null;

    public int? IntValue => int.TryParse(Value, out var parsed)
        ? parsed
        : null;

    public static bool IsTokenLine(string line)
    {
        return TokenLineRegex().IsMatch(line);
    }

    public static OfxRawToken ParseLine(string line)
    {
        var match = TokenLineRegex().Match(line);
        return new OfxRawToken
        {
            Name = match.Groups["Name"].Value.Trim().ToUpper(),
            Value = match.Groups["Value"].Value.Trim(),
        };
    }

    [GeneratedRegex(@"^\<(?'Name'[\w\d\.]+)\>(?'Value'.+)$")]
    private static partial Regex TokenLineRegex();
}
