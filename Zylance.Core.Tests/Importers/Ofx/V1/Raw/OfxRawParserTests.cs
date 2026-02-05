using Zylance.Core.Lib.Extensions;
using Zylance.Core.Tests.Fixtures;
using OfxRawElement = Zylance.Core.Importers.Ofx.V1.Raw.OfxRawElement;
using OfxRawFile = Zylance.Core.Importers.Ofx.V1.Raw.OfxRawFile;
using OfxRawToken = Zylance.Core.Importers.Ofx.V1.Raw.OfxRawToken;

namespace Zylance.Core.Tests.Importers.Ofx.V1.Raw;

public class OfxRawParserTests
{
    [Fact]
    public void OfxRawFile_Parse_ParsesHeadersCorrectly()
    {
        // Arrange
        var ofxContent =
            @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
SECURITY:NONE

<OFX>
  <SIGNONMSGSRSV1>
    <SONRS>
      <STATUS>
        <CODE>0
      </STATUS>
    </SONRS>
  </SIGNONMSGSRSV1>
</OFX>";

        // Act
        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var rawFile = OfxRawFile.Parse(reader);

        // Assert
        Assert.Equal(4, rawFile.Headers.Count);
        Assert.Contains(rawFile.Headers, h => h.Name == "OFXHEADER" && h.Value == "100");
        Assert.Contains(rawFile.Headers, h => h.Name == "DATA" && h.Value == "OFXSGML");
        Assert.Contains(rawFile.Headers, h => h.Name == "VERSION" && h.Value == "102");
        Assert.Contains(rawFile.Headers, h => h.Name == "SECURITY" && h.Value == "NONE");
    }

    [Fact]
    public void OfxRawFile_Parse_ParsesRootElement()
    {
        // Arrange
        var ofxContent =
            @"OFXHEADER:100
DATA:OFXSGML

<OFX>
  <BANKMSGSRSV1>
  </BANKMSGSRSV1>
</OFX>";

        // Act
        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var rawFile = OfxRawFile.Parse(reader);

        // Assert
        Assert.Equal("OFX", rawFile.Root.Name);
        Assert.Single(rawFile.Root.Children);
        Assert.Equal("BANKMSGSRSV1", rawFile.Root.Children[0].Name);
    }

    [Fact]
    public void OfxRawElement_ParseElement_ParsesTokens()
    {
        // Arrange
        var ofxContent =
            @"<BANKACCTFROM>
  <BANKID>123456789
  <ACCTID>9876543210
  <ACCTTYPE>CHECKING
</BANKACCTFROM>
";

        // Act
        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var line = reader.ReadLine();
        Assert.NotNull(line);
        var element = OfxRawElement.ParseElement(line, reader);

        // Assert
        Assert.Equal("BANKACCTFROM", element.Name);
        Assert.Equal(3, element.Tokens.Count);
        Assert.Equal("123456789", element.Tokens["BANKID"].Value);
        Assert.Equal("9876543210", element.Tokens["ACCTID"].Value);
        Assert.Equal("CHECKING", element.Tokens["ACCTTYPE"].Value);
    }

    [Fact]
    public void OfxRawElement_ParseElement_HandlesNestedElements()
    {
        // Arrange
        var ofxContent =
            @"<STMTRS>
  <CURDEF>USD
  <BANKACCTFROM>
    <BANKID>123
  </BANKACCTFROM>
</STMTRS>
";

        // Act
        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var line = reader.ReadLine();
        Assert.NotNull(line);
        var element = OfxRawElement.ParseElement(line, reader);

        // Assert
        Assert.Equal("STMTRS", element.Name);
        Assert.Single(element.Tokens);
        Assert.Equal("USD", element.Tokens["CURDEF"].Value);
        Assert.Single(element.Children);
        Assert.Equal("BANKACCTFROM", element.Children[0].Name);
        Assert.Equal("123", element.Children[0].Tokens["BANKID"].Value);
    }

    [Fact]
    public void OfxRawToken_ParseLine_ParsesDateTimeValue()
    {
        // Arrange
        var line = "<DTPOSTED>20260202120000[0:GMT]";

        // Act
        var token = OfxRawToken.ParseLine(line);

        // Assert
        Assert.Equal("DTPOSTED", token.Name);
        Assert.Equal("20260202120000[0:GMT]", token.Value);
        Assert.Equal("2026-02-02T12:00:00.000+00:00", token.DateTimeValue.ToIso8601());
    }

    [Fact]
    public void OfxRawToken_ParseLine_ParsesDecimalValue()
    {
        // Arrange
        var line = "<TRNAMT>-87.50";

        // Act
        var token = OfxRawToken.ParseLine(line);

        // Assert
        Assert.Equal("TRNAMT", token.Name);
        Assert.Equal("-87.50", token.Value);
        Assert.Equal(-87.50m, token.DecimalValue);
    }

    [Fact]
    public void OfxRawToken_IsTokenLine_ReturnsTrueForValidToken()
    {
        // Arrange
        var line = "<BANKID>123456789";

        // Act
        var isToken = OfxRawToken.IsTokenLine(line);

        // Assert
        Assert.True(isToken);
    }

    [Fact]
    public void OfxRawElement_IsStartLine_ReturnsTrueForValidStartTag()
    {
        // Arrange
        var line = "<BANKACCTFROM>";

        // Act
        var isStart = OfxRawElement.IsStartLine(line);

        // Assert
        Assert.True(isStart);
    }

    [Fact]
    public void OfxRawElement_GetChildElement_ReturnsCorrectChild()
    {
        // Arrange
        var ofxContent =
            @"<STMTRS>
  <BANKACCTFROM>
    <BANKID>123
  </BANKACCTFROM>
  <LEDGERBAL>
    <BALAMT>100.00
  </LEDGERBAL>
</STMTRS>
";

        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var line = reader.ReadLine();
        Assert.NotNull(line);
        var element = OfxRawElement.ParseElement(line, reader);

        // Act
        var child = element.GetChildElement("LEDGERBAL");

        // Assert
        Assert.Equal("LEDGERBAL", child.Name);
        Assert.Equal("100.00", child.Tokens["BALAMT"].Value);
    }

    [Fact]
    public void OfxRawElement_GetChildElement_ThrowsWhenNotFound()
    {
        // Arrange
        var ofxContent =
            @"<STMTRS>
  <BANKACCTFROM>
    <BANKID>123
  </BANKACCTFROM>
</STMTRS>
";

        using var reader = FixtureUtils.StringToStreamReader(ofxContent);
        var line = reader.ReadLine();
        Assert.NotNull(line);
        var element = OfxRawElement.ParseElement(line, reader);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => element.GetChildElement("NONEXISTENT"));
    }
}
