using Zylance.Core.Lib.Importers.Ofx.Parser;
using Zylance.Core.Tests.Fixtures;

namespace Zylance.Core.Tests.Importers.Ofx.Parser;

public class OfxParserTests
{
    [Fact]
    public async Task ParseAsync_WithExampleOfx_ParsesCorrectly()
    {
        // Arrange
        var parser = new OfxParser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.ofx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.Equal("999888777", statement.Account.BankId);
        Assert.Equal("1122334455", statement.Account.AccountId);
        Assert.Equal("SAVINGS", statement.Account.AccountType);
        Assert.Equal("CAD", statement.Account.Currency);
        Assert.Equal("BANK", statement.Account.Type);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_ParsesCorrectly()
    {
        // Arrange
        var parser = new OfxParser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.Equal("123456789", statement.Account.BankId);
        Assert.Equal("9876543210", statement.Account.AccountId);
        Assert.Equal("CHECKING", statement.Account.AccountType);
        Assert.Equal("USD", statement.Account.Currency);
        Assert.Equal("BANK", statement.Account.Type);
    }

    [Fact]
    public async Task ParseAsync_WithTransferOfx_ParsesTransferFlag()
    {
        // Arrange
        var parser = new OfxParser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/transfer.ofx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var transaction = statements[0].Transactions[0];
        Assert.True(transaction.IsTransfer);
        Assert.Equal("XFER", transaction.Type);
    }

    [Fact]
    public async Task ParseAsync_WithMultipleTransactions_ParsesAllTransactions()
    {
        // Arrange
        var parser = new OfxParser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var statement = statements[0];
        Assert.Equal(10, statement.Transactions.Count);
    }

    [Fact]
    public async Task ParseAsync_WithBalances_ParsesBalancesCorrectly()
    {
        // Arrange
        var parser = new OfxParser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var statement = statements[0];
        Assert.NotNull(statement.LedgerBalance);
        Assert.Equal(1411.81m, statement.LedgerBalance.Amount);
        Assert.Equal("LEDGER", statement.LedgerBalance.Type);
    }
}
