using Zylance.Core.Lib.Extensions;
using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.V1.Parser;
using Zylance.Core.Tests.Fixtures;

namespace Zylance.Core.Tests.Importers.Ofx.V1.Parser;

public class OfxV1ParserTests
{
    [Fact]
    public async Task ParseAsync_WithExampleQfx_ParsesStatement()
    {
        // Arrange
        var parser = new OfxV1Parser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.NotNull(statement.Account);
        Assert.Equal("123456789", statement.Account.BankId);
        Assert.Equal("9876543210", statement.Account.AccountId);
        Assert.Equal("CHECKING", statement.Account.AccountType);
        Assert.Equal("USD", statement.Account.Currency);
        Assert.Equal("BANK", statement.Account.Type);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_ParsesTransactions()
    {
        // Arrange
        var parser = new OfxV1Parser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var statement = statements[0];
        Assert.Equal(10, statement.Transactions.Count);

        var firstTransaction = statement.Transactions[0];
        Assert.Equal("DEBIT", firstTransaction.Type);
        Assert.Equal("2025-01-02T12:00:00.0000000+00:00", firstTransaction.DatePosted.ToIsoTimestamp());
        Assert.Equal(-87.50m, firstTransaction.Amount);
        Assert.Equal("2025010201", firstTransaction.FitId);
        Assert.Equal("WHOLE FOODS MARKET #123", firstTransaction.Name);
        Assert.Equal("Grocery shopping", firstTransaction.Memo);

        var creditTransaction = statement.Transactions.First(t => t.Type == "CREDIT");
        Assert.Equal(2500.00m, creditTransaction.Amount);
        Assert.Equal("DIRECT DEPOSIT ACME CORP", creditTransaction.Name);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_ParsesBalances()
    {
        // Arrange
        var parser = new OfxV1Parser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var statement = statements[0];
        Assert.NotNull(statement.LedgerBalance);
        Assert.Equal(1411.81m, statement.LedgerBalance.Amount);
        Assert.Equal(new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero), statement.LedgerBalance.AsOfDate);
        Assert.Equal("LEDGER", statement.LedgerBalance.Type);

        Assert.NotNull(statement.AvailableBalance);
        Assert.Equal(1411.81m, statement.AvailableBalance.Amount);
        Assert.Equal("AVAIL", statement.AvailableBalance.Type);
    }

    [Fact]
    public async Task ParseAsync_WithExampleOfx_ParsesCorrectly()
    {
        // Arrange
        var parser = new OfxV1Parser();

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
        
        Assert.Equal(2, statement.Transactions.Count);

        var credit = statement.Transactions.First(t => t.Type == "CREDIT");
        Assert.Equal(500.00m, credit.Amount);
        Assert.Equal("SALARY DEPOSIT", credit.Name);

        var debit = statement.Transactions.First(t => t.Type == "DEBIT");
        Assert.Equal(-75.25m, debit.Amount);
        Assert.Equal("GROCERY STORE", debit.Name);
    }

    [Fact]
    public async Task ParseAsync_TransactionWithXferType_SetsIsTransferFlag()
    {
        // Arrange
        var parser = new OfxV1Parser();

        // Act
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/transfer.ofx");
        var statements = await parser.ParseAsync(reader);

        // Assert
        var transaction = statements[0].Transactions[0];
        Assert.True(transaction.IsTransfer);
        Assert.Equal("XFER", transaction.Type);
    }
}
