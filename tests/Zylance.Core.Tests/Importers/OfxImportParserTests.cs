using Zylance.Core.Importers.Ofx;
using Zylance.Core.Tests.Fixtures;

namespace Zylance.Core.Tests.Importers;

public class OfxImportParserTests
{
    private readonly OfxImportParser _importParser = new();

    [Fact]
    public async Task ParseAsync_WithExampleOfx_ParsesCorrectly()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture(Path.Combine("Importers", "Ofx", "V1", "example.ofx"));

        // Act
        var result = await _importParser.ParseAsync(reader.BaseStream, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.TransactionCount);
        Assert.NotEmpty(result.Statements);
        var statement = result.Statements.First();
        Assert.Equal("1122334455", statement.Account.Id);
        Assert.Equal(2, statement.Transactions.Count);
    }

    [Fact]
    public async Task ParseAsync_WithCreditCardOfx_ParsesCorrectly()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture(Path.Combine("Importers", "Ofx", "V1", "creditcard.ofx"));

        // Act
        var result = await _importParser.ParseAsync(reader.BaseStream, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(3, result.TransactionCount);
        Assert.NotEmpty(result.Statements);
        var statement = result.Statements.First();
        Assert.Equal("4111111111111111", statement.Account.Id);
        Assert.Equal(3, statement.Transactions.Count);
    }
}
