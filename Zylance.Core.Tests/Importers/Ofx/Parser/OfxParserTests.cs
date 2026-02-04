using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.Parser;

namespace Zylance.Core.Tests.Importers.Ofx.Parser;

public class OfxParserTests
{
    [Fact]
    public async Task ParseAsync_WithExampleQfx_EmitsExpectedBankAccount()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        OfxBankAccount? capturedAccount = null;

        var parser = new OfxParser();
        parser.HandleAccount(account =>
        {
            capturedAccount = account;
            return Task.CompletedTask;
        });

        // Act
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);

        // Assert
        Assert.NotNull(capturedAccount);
        Assert.Equal("123456789", capturedAccount.BankId);
        Assert.Equal("9876543210", capturedAccount.AccountId);
        Assert.Equal("CHECKING", capturedAccount.AccountType);
        Assert.Equal("USD", capturedAccount.Currency);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_EmitsExpectedTransactionCount()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        var transactions = new List<OfxTransaction>();

        var parser = new OfxParser();
        parser.HandleTransaction(transaction =>
        {
            transactions.Add(transaction);
            return Task.CompletedTask;
        });

        // Act
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);

        // Assert
        Assert.Equal(10, transactions.Count);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_EmitsTransactionsWithCorrectData()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        var transactions = new List<OfxTransaction>();

        var parser = new OfxParser();
        parser.HandleTransaction(transaction =>
        {
            transactions.Add(transaction);
            return Task.CompletedTask;
        });

        // Act
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);

        // Assert - Check first transaction
        var firstTransaction = transactions[0];
        Assert.Equal("DEBIT", firstTransaction.Type);
        Assert.Equal(new DateTimeOffset(2025, 1, 2, 12, 0, 0, TimeSpan.Zero), firstTransaction.DatePosted);
        Assert.Equal(-87.50m, firstTransaction.Amount);
        Assert.Equal("2025010201", firstTransaction.FitId);
        Assert.Equal("WHOLE FOODS MARKET #123", firstTransaction.Name);
        Assert.Equal("Grocery shopping", firstTransaction.Memo);

        // Assert - Check a credit transaction
        var creditTransaction = transactions.First(t => t.Type == "CREDIT");
        Assert.Equal("CREDIT", creditTransaction.Type);
        Assert.Equal(2500.00m, creditTransaction.Amount);
        Assert.Equal("DIRECT DEPOSIT ACME CORP", creditTransaction.Name);
    }

    [Fact]
    public async Task ParseAsync_WithExampleQfx_EmitsExpectedBalances()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        var balances = new List<OfxBalance>();

        var parser = new OfxParser();
        parser.HandleBalance(balance =>
        {
            balances.Add(balance);
            return Task.CompletedTask;
        });

        // Act
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);

        // Assert
        Assert.Equal(2, balances.Count);

        var ledgerBalance = balances.First(b => b.Type == "LEDGER");
        Assert.Equal(1411.81m, ledgerBalance.Amount);
        Assert.Equal(new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero), ledgerBalance.AsOfDate);

        var availBalance = balances.First(b => b.Type == "AVAIL");
        Assert.Equal(1411.81m, availBalance.Amount);
        Assert.Equal(new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero), availBalance.AsOfDate);
    }

    [Fact]
    public async Task ParseAsync_WithNoHandlers_CompletesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        var parser = new OfxParser();

        // Act & Assert - Should not throw
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);
    }

    [Fact]
    public async Task ParseAsync_MultipleHandlerCalls_AllExecuted()
    {
        // Arrange
        var filePath = Path.Combine("Importers", "Fixtures", "example.qfx");
        var accountCount = 0;
        var transactionCount = 0;
        var balanceCount = 0;

        var parser = new OfxParser();
        parser.HandleAccount(_ =>
        {
            accountCount++;
            return Task.CompletedTask;
        });
        parser.HandleTransaction(_ =>
        {
            transactionCount++;
            return Task.CompletedTask;
        });
        parser.HandleBalance(_ =>
        {
            balanceCount++;
            return Task.CompletedTask;
        });

        // Act
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);
        await parser.ParseAsync(reader);

        // Assert
        Assert.Equal(1, accountCount);
        Assert.Equal(10, transactionCount);
        Assert.Equal(2, balanceCount);
    }
}
