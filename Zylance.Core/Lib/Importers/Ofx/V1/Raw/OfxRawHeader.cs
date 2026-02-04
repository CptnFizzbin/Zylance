using System.Text.RegularExpressions;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Raw;

internal partial record OfxRawHeader
{
    public required string Name { get; init; }
    public required string Value { get; init; }

    public static OfxRawHeader ParseLine(string line)
    {
        var split = line.Split(':', 2);

        return new OfxRawHeader
        {
            Name = split[0].Trim().ToUpper(),
            Value = split[1].Trim(),
        };
    }

    [GeneratedRegex(@"^(?'Name'[\w\d\.]+):(?'Value'.+)$")]
    private static partial Regex HeaderLineRegex();

    public static bool IsMatch(string line)
    {
        return HeaderLineRegex().IsMatch(line);
    }
}
