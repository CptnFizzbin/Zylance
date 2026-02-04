using System.Text.RegularExpressions;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Parser;

internal static class DateTimeOffsetParser
{
    // NOTE: Not using [GeneratedRegex] for this pattern due to its complexity
    private readonly static Lazy<Regex> DateTimeRegex = new(() =>
    {
        // OFX date-time format: YYYYMMDDHHMMSS.XXX[gmt offset:tz name] or YYYYMMDD (date only)
        // Example: 20220101123000.000[-5:EST] or 20220101
        // Time components must be complete (all 6 digits HHMMSS) or absent
        var pattern = string.Join(
            "",
            @"(?'Year'\d{4})",
            @"(?'Month'\d{2})",
            @"(?'Day'\d{2})",
            @"((?'Hour'\d{2})(?'Minute'\d{2})(?'Second'\d{2}))?", // Time is all-or-nothing
            @"(\.(?'Fraction'\d+))?",
            @"(\[(?'Offset'[-+]?\d+):.*\])?" // Allow optional sign for offset
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
            
            // Time components default to 0 if not present (midnight GMT)
            var hour = match.Groups["Hour"].Success ? int.Parse(match.Groups["Hour"].Value) : 0;
            var minute = match.Groups["Minute"].Success ? int.Parse(match.Groups["Minute"].Value) : 0;
            var second = match.Groups["Second"].Success ? int.Parse(match.Groups["Second"].Value) : 0;
            
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
