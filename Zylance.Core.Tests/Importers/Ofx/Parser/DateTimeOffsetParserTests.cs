using Zylance.Core.Lib.Importers.Ofx.V1.Parser;

namespace Zylance.Core.Tests.Importers.Ofx.Parser;

public class DateTimeOffsetParserTests
{
    [Theory]
    // Basic format without timezone
    [InlineData("20220101123000", "2022-01-01T12:30:00+00:00")]
    [InlineData("20231215083045", "2023-12-15T08:30:45+00:00")]
    [InlineData("19990630235959", "1999-06-30T23:59:59+00:00")]
    [InlineData("20240229120000", "2024-02-29T12:00:00+00:00")] // Leap year
    [InlineData("20220101000000", "2022-01-01T00:00:00+00:00")] // Midnight
    [InlineData("20221231235959", "2022-12-31T23:59:59+00:00")] // End of day
    // Date-only format (YYYYMMDD) - time assumed to be 00:00 GMT
    [InlineData("20220101", "2022-01-01T00:00:00+00:00")]
    [InlineData("20231215", "2023-12-15T00:00:00+00:00")]
    [InlineData("19990630", "1999-06-30T00:00:00+00:00")]
    [InlineData("20240229", "2024-02-29T00:00:00+00:00")] // Leap year
    // With fractional seconds
    [InlineData("20220101123000.000", "2022-01-01T12:30:00+00:00")]
    [InlineData("20231215083045.123", "2023-12-15T08:30:45+00:00")]
    [InlineData("19990630235959.999", "1999-06-30T23:59:59+00:00")]
    [InlineData("20240101000000.0", "2024-01-01T00:00:00+00:00")]
    [InlineData("20220101123000.123456789", "2022-01-01T12:30:00+00:00")] // Long fraction
    // With timezone offsets
    [InlineData("20220101123000[-5:EST]", "2022-01-01T12:30:00-05:00")]
    [InlineData("20220101123000[+8:PST]", "2022-01-01T12:30:00+08:00")]
    [InlineData("20220101123000[-7:MST]", "2022-01-01T12:30:00-07:00")]
    [InlineData("20220101123000[0:GMT]", "2022-01-01T12:30:00+00:00")]
    [InlineData("20220101123000[+12:NZST]", "2022-01-01T12:30:00+12:00")]
    [InlineData("20220101123000[+5:EST]", "2022-01-01T12:30:00+05:00")]
    [InlineData("20220101123000[-10:HST]", "2022-01-01T12:30:00-10:00")]
    [InlineData("20220101123000[+14:LINT]", "2022-01-01T12:30:00+14:00")]
    [InlineData("20220101123000[-5:US/Eastern]", "2022-01-01T12:30:00-05:00")] // Complex timezone name
    // With fractional seconds and timezone
    [InlineData("20220101123000.000[-5:EST]", "2022-01-01T12:30:00-05:00")]
    [InlineData("20231215083045.123[+8:PST]", "2023-12-15T08:30:45+08:00")]
    [InlineData("19990630235959.999[-7:MST]", "1999-06-30T23:59:59-07:00")]
    [InlineData("20240229120000.0[0:GMT]", "2024-02-29T12:00:00+00:00")]
    [InlineData("20220101123000.000[+8:PST]", "2022-01-01T12:30:00+08:00")]
    public void TryParse_ValidInput_ParsesCorrectly(string ofxTimestamp, string expectedIso8601)
    {
        // Act
        var result = DateTimeOffsetParser.TryParse(ofxTimestamp, out var dto);

        // Assert
        Assert.True(result);
        Assert.Equal(DateTimeOffset.Parse(expectedIso8601), dto);
    }

    [Theory]
    [InlineData("")] // Empty
    [InlineData(" ")] // Whitespace
    [InlineData("not-a-date")] // Invalid format
    [InlineData("2022-01-01")] // ISO format
    [InlineData("01/01/2022")] // US format
    [InlineData("2022")] // Year only
    [InlineData("202201")] // Year and month only (missing day)
    [InlineData("202201011230")] // Incomplete time
    [InlineData("2022010112300")] // Missing one digit
    [InlineData("202201011230000")] // Extra digit
    [InlineData("20220001123000")] // Invalid month (0)
    [InlineData("20221301123000")] // Invalid month (13)
    [InlineData("20220100123000")] // Invalid day (0)
    [InlineData("20220132123000")] // Invalid day (32)
    [InlineData("20220229120000")] // Feb 29 on non-leap year
    [InlineData("20220431120000")] // April 31 (doesn't exist)
    [InlineData("20220101250000")] // Invalid hour (25)
    [InlineData("20220101126000")] // Invalid minute (60)
    [InlineData("20220101123060")] // Invalid second (60)
    public void TryParse_InvalidInput_ReturnsFalse(string ofxTimestamp)
    {
        // Act
        var result = DateTimeOffsetParser.TryParse(ofxTimestamp, out var dto);

        // Assert
        Assert.False(result);
        Assert.Equal(default, dto);
    }
}
