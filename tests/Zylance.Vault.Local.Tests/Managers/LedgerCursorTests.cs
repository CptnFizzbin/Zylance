using Zylance.Vault.Local.Managers;

namespace Zylance.Vault.Local.Tests.Managers;

/// <summary>
///     Tests for LedgerCursor encoding and decoding.
/// </summary>
public class LedgerCursorTests
{
    [Fact]
    public void EncodeDecode_ValidCursor_RoundTripsSuccessfully()
    {
        // Arrange
        var timestamp = DateTimeOffset.Parse("2020-08-04T10:10:10Z");
        var id = Guid.NewGuid();
        var cursor = new LedgerCursor { Timestamp = timestamp, Id = id };

        // Act
        var encoded = cursor.Encode();
        var decoded = LedgerCursor.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Equal(timestamp, decoded.Timestamp);
        Assert.Equal(id, decoded.Id);
    }
}
