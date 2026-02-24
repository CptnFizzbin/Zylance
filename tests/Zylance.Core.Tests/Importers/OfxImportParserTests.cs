using Zylance.Core.Importers.Ofx;
using Zylance.Core.Tests.TestUtils.Fixtures;

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
        // Ensure transaction FITIDs are mapped to TrxId on imported ledger entries
        Assert.Equal("2026020201", statement.Transactions[0].TrxId);
        Assert.Equal("2026020301", statement.Transactions[1].TrxId);
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
        // Ensure transaction FITIDs are mapped to TrxId on imported ledger entries
        Assert.Equal("CC2026020201", statement.Transactions[0].TrxId);
        Assert.Equal("CC2026020301", statement.Transactions[1].TrxId);
        Assert.Equal("CC2026020401", statement.Transactions[2].TrxId);
    }
}
