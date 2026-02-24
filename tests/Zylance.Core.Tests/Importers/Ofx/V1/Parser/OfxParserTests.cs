using Zylance.Core.Importers.Ofx.V1;
using Zylance.Core.Tests.TestUtils.Fixtures;

namespace Zylance.Core.Tests.Importers.Ofx.V1.Parser;

public class OfxV1ParserTests
{
    [Fact]
    public Task ParseAsync_WithExampleQfx_ParsesStatement()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.NotNull(statement.Account);
        Assert.Equal("123456789", statement.Account.BankId);
        Assert.Equal("9876543210", statement.Account.AccountId);
        Assert.Equal("CHECKING", statement.Account.AccountType);
        Assert.Equal("USD", statement.Account.Currency);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithExampleQfx_ParsesTransactions()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var statement = statements[0];
        Assert.Equal(10, statement.Transactions.Count);

        var firstTransaction = statement.Transactions[0];
        Assert.Equal("DEBIT", firstTransaction.Type);
        Assert.Equal("2025-01-02T12:00:00.0000000+00:00", firstTransaction.DatePosted.ToString("O"));
        Assert.Equal(-87.50m, firstTransaction.Amount);
        Assert.Equal("2025010201", firstTransaction.Id);
        Assert.Equal("WHOLE FOODS MARKET #123", firstTransaction.Name);
        Assert.Equal("Grocery shopping", firstTransaction.Memo);

        var creditTransaction = statement.Transactions.First(t => t.Type == "CREDIT");
        Assert.Equal(2500.00m, creditTransaction.Amount);
        Assert.Equal("DIRECT DEPOSIT ACME CORP", creditTransaction.Name);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithExampleQfx_ParsesBalances()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.qfx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var statement = statements[0];
        Assert.NotNull(statement.LedgerBalance);
        Assert.Equal(1411.81m, statement.LedgerBalance.Amount);
        Assert.Equal("2025-01-15T12:00:00.0000000+00:00", statement.LedgerBalance.AsOfDate.ToString("O"));
        Assert.Equal("LEDGER", statement.LedgerBalance.Type);

        Assert.NotNull(statement.AvailableBalance);
        Assert.Equal(1411.81m, statement.AvailableBalance.Amount);
        Assert.Equal("AVAIL", statement.AvailableBalance.Type);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithExampleOfx_ParsesCorrectly()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/example.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

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
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_TransactionWithXferType_SetsIsTransferFlag()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/transfer.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var transaction = statements[0].Transactions[0];
        Assert.True(transaction.IsTransfer);
        Assert.Equal("XFER", transaction.Type);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithCreditCardOfx_ParsesStatement()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/creditcard.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.NotNull(statement.Account);
        Assert.Null(statement.Account.BankId); // Credit cards don't have bank IDs
        Assert.Equal("4111111111111111", statement.Account.AccountId);
        Assert.Equal("CREDITCARD", statement.Account.AccountType);
        Assert.Equal("USD", statement.Account.Currency);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithCreditCardOfx_ParsesTransactions()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/creditcard.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var statement = statements[0];
        Assert.Equal(3, statement.Transactions.Count);

        var firstDebit = statement.Transactions[0];
        Assert.Equal("DEBIT", firstDebit.Type);
        Assert.Equal("2026-02-02T12:00:00.0000000+00:00", firstDebit.DatePosted.ToString("O"));
        Assert.Equal(-125.50m, firstDebit.Amount);
        Assert.Equal("CC2026020201", firstDebit.Id);
        Assert.Equal("ONLINE RETAILER", firstDebit.Name);
        Assert.Equal("Purchase at online store", firstDebit.Memo);

        var creditTransaction = statement.Transactions.First(t => t.Type == "CREDIT");
        Assert.Equal(200.00m, creditTransaction.Amount);
        Assert.Equal("PAYMENT RECEIVED", creditTransaction.Name);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithCreditCardOfx_ParsesBalances()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/creditcard.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var statement = statements[0];
        Assert.NotNull(statement.LedgerBalance);
        Assert.Equal(-971.49m, statement.LedgerBalance.Amount);
        Assert.Equal("2026-02-04T12:00:00.0000000+00:00", statement.LedgerBalance.AsOfDate.ToString("O"));
        Assert.Equal("LEDGER", statement.LedgerBalance.Type);

        Assert.NotNull(statement.AvailableBalance);
        Assert.Equal(4028.51m, statement.AvailableBalance.Amount);
        Assert.Equal("AVAIL", statement.AvailableBalance.Type);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_TransactionWithMissingMemo_ParsesWithNullMemo()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/ofx-missing-memo.ofx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        var statement = statements[0];
        Assert.Single(statement.Transactions);
        var transaction = statement.Transactions[0];
        Assert.Equal("DEBIT", transaction.Type);
        Assert.Equal("2026-01-02T12:00:00.0000000+00:00", transaction.DatePosted.ToString("O"));
        Assert.Equal(-50.00m, transaction.Amount);
        Assert.Equal("2026010201", transaction.Id);
        Assert.Equal("NO MEMO TRANSACTION", transaction.Name);
        Assert.Null(transaction.Memo); // Memo should be null if missing
        return Task.CompletedTask;
    }

    [Fact]
    public Task ParseAsync_WithSingleLineQfx_ParsesCorrectly()
    {
        // Arrange
        using var reader = FixtureUtils.LoadFixture("Importers/Ofx/V1/single-line.qfx");

        // Act
        var statements = OfxV1Parser.Parse(reader);

        // Assert
        Assert.Single(statements);
        var statement = statements[0];
        Assert.NotNull(statement.Account);
        Assert.Equal("123456789", statement.Account.BankId);
        Assert.Equal("9876543210", statement.Account.AccountId);
        Assert.Equal("CHECKING", statement.Account.AccountType);
        Assert.Equal("USD", statement.Account.Currency);
        Assert.Equal(10, statement.Transactions.Count);
        Assert.NotNull(statement.LedgerBalance);
        Assert.Equal(1411.81m, statement.LedgerBalance.Amount);
        Assert.Equal("LEDGER", statement.LedgerBalance.Type);
        Assert.NotNull(statement.AvailableBalance);
        Assert.Equal(1411.81m, statement.AvailableBalance.Amount);
        Assert.Equal("AVAIL", statement.AvailableBalance.Type);
        return Task.CompletedTask;
    }
}
