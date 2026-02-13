using System.Globalization;
using System.Text.RegularExpressions;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal static partial class OfxTimeStamp
{
    private static readonly CultureInfo EnUs = new("en-US");

    private static readonly string[] ValidFormats = ["yyyyMMdd", "yyyyMMddHHmmss", "yyyyMMddHHmmss.FFF"];

    [GeneratedRegex(@"^(?'DateTime'\d+(?:\.\d+)?)(?:\[(?'Offset'[+-]?\d+(?:\.\d+)?)(?::\w+)?\])?$")]
    private static partial Regex OfxDateTimeRegex();

    public static bool TryParse(string input, out DateTimeOffset value)
    {
        try
        {
            var match = OfxDateTimeRegex().Match(input);
            if (!match.Success)
            {
                value = default;
                return false;
            }

            var dateTimePart = match.Groups["DateTime"].Value;
            var dateTime = DateTime.ParseExact(dateTimePart, ValidFormats, EnUs);

            if (match.Groups["Offset"].Success)
            {
                var offsetPart = match.Groups["Offset"].Value;
                if (double.TryParse(offsetPart, NumberStyles.Float, EnUs, out var offsetHours))
                {
                    var offset = TimeSpan.FromHours(offsetHours);
                    value = new DateTimeOffset(dateTime, offset);
                    return true;
                }
            }

            value = new DateTimeOffset(dateTime, TimeSpan.Zero);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    public static DateTimeOffset Parse(string value)
    {
        return TryParse(value, out var result)
            ? result
            : throw new FormatException($"Invalid OFX timestamp format: {value}");
    }
}
