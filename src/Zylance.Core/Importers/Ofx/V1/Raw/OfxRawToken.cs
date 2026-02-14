using System.Text.RegularExpressions;
using OfxTimeStamp = Zylance.Core.Importers.Ofx.V1.Models.OfxTimeStamp;

namespace Zylance.Core.Importers.Ofx.V1.Raw;

internal partial record OfxRawToken
{
    public required string Name { get; init; }
    public required string Value { get; init; }

    public DateTimeOffset DateTimeValue => OfxTimeStamp.Parse(Value);

    public DateTimeOffset? TryDateTimeValue => OfxTimeStamp.TryParse(Value, out var parsed) ? parsed : null;

    public decimal DecimalValue => decimal.Parse(Value);

    public decimal? TryDecimalValue => decimal.TryParse(Value, out var parsed) ? parsed : null;

    public int IntValue => int.Parse(Value);

    public int? TryIntValue => int.TryParse(Value, out var parsed) ? parsed : null;

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
