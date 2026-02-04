using System.Text.RegularExpressions;

namespace Zylance.Core.Lib.Importers.Ofx.Parser;

internal class DateTimeOffsetParser
{
    // NOTE: Not using [GeneratedRegex] for this pattern due to its complexity
    private readonly static Lazy<Regex> DateTimeRegex = new(() =>
    {
        // OFX date-time format: YYYYMMDDHHMMSS.XXX[gmt offset:tz name]
        // Example: 20220101123000.000[-5:EST]
        var pattern = string.Join(
            "",
            @"(?'Year'\d{4})",
            @"(?'Month'\d{2})",
            @"(?'Day'\d{2})",
            @"(?'Hour'\d{2})",
            @"(?'Minute'\d{2})",
            @"(?'Second'\d{2})",
            @"(\.(?'Fraction'\d+))?",
            @"(\[(?'Offset'[-+]\d+):.*\])?"
        );

        return new Regex($"^{pattern}$");
    });

    public static bool TryParse(string input, out DateTimeOffset dto)
    {
        var match = DateTimeRegex.Value.Match(input);
        if (!match.Success)
        {
            dto = default;
            return false;
        }

        try
        {
            var year = int.Parse(match.Groups["Year"].Value);
            var month = int.Parse(match.Groups["Month"].Value);
            var day = int.Parse(match.Groups["Day"].Value);
            var hour = int.Parse(match.Groups["Hour"].Value);
            var minute = int.Parse(match.Groups["Minute"].Value);
            var second = int.Parse(match.Groups["Second"].Value);
            var offsetHours = match.Groups["Offset"].Success
                ? int.Parse(match.Groups["Offset"].Value)
                : 0;

            var offset = TimeSpan.FromHours(offsetHours);
            dto = new DateTimeOffset(year, month, day, hour, minute, second, offset);
            return true;
        }
        catch
        {
            dto = default;
            return false;
        }
    }
}
