using Zylance.Core.Vault.Models;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Managers;
using Zylance.Vault.Local.Tests.Factories;

namespace Zylance.Vault.Local.Tests.Managers;

/// <summary>
///     Tests for LocalAccountManager to ensure CRUD operations and listing work correctly.
/// </summary>
public class LocalAccountManagerTests : IDisposable
{
    private readonly LocalVaultDbContext _context;
    private readonly LocalAccountManager _manager;

    public LocalAccountManagerTests()
    {
        _context = TestDbContextFactory.CreateContext();
        _manager = new LocalAccountManager(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Helper Methods

    private async Task<Guid> SeedAccount(string name, string type, decimal balance)
    {
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = name,
            Type = type,
            Balance = balance,
        };
        await _manager.SaveAsync(account);
        return accountId;
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Test Checking",
            Type = "checking",
            Balance = 1500.50M,
        };
        await _manager.SaveAsync(account);

        // Act
        var result = await _manager.GetAsync(accountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId.ToString(), result.Id);
        Assert.Equal("Test Checking", result.Name);
        Assert.Equal("checking", result.Type);
        Assert.Equal(1500.50M, result.Balance);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.GetAsync(invalidId));
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WithNewAccount_CreatesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "New Savings",
            Type = "savings",
            Balance = 5000.00M,
        };

        // Act
        var result = await _manager.SaveAsync(account);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId.ToString(), result.Id);
        Assert.Equal("New Savings", result.Name);
        Assert.Equal("savings", result.Type);
        Assert.Equal(5000.00M, result.Balance);

        // Verify it was persisted
        var retrieved = await _manager.GetAsync(accountId);
        Assert.Equal("New Savings", retrieved.Name);
    }

    [Fact]
    public async Task SaveAsync_WithExistingAccount_UpdatesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Original Name",
            Type = "checking",
            Balance = 1000.00M,
        };
        await _manager.SaveAsync(account);

        // Act - Update the account
        var updated = new AccountModel
        {
            Id = account.Id,
            Name = "Updated Name",
            Type = account.Type,
            Balance = 2000.00M,
        };
        var result = await _manager.SaveAsync(updated);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(2000.00M, result.Balance);

        // Verify it was updated
        var retrieved = await _manager.GetAsync(accountId);
        Assert.Equal("Updated Name", retrieved.Name);
        Assert.Equal(2000.00M, retrieved.Balance);
    }

    [Fact]
    public async Task SaveAsync_WithDifferentAccountTypes_PreservesType()
    {
        // Arrange & Act
        var checkingId = Guid.NewGuid();
        var savingsId = Guid.NewGuid();
        var creditId = Guid.NewGuid();

        await _manager.SaveAsync(
            new AccountModel
            {
                Id = checkingId.ToString(),
                Name = "Checking",
                Type = "checking",
                Balance = 500M,
            }
        );

        await _manager.SaveAsync(
            new AccountModel
            {
                Id = savingsId.ToString(),
                Name = "Savings",
                Type = "savings",
                Balance = 10000M,
            }
        );

        await _manager.SaveAsync(
            new AccountModel
            {
                Id = creditId.ToString(),
                Name = "Credit Card",
                Type = "credit",
                Balance = -500M,
            }
        );

        // Assert
        var checking = await _manager.GetAsync(checkingId);
        var savings = await _manager.GetAsync(savingsId);
        var credit = await _manager.GetAsync(creditId);

        Assert.Equal("checking", checking.Type);
        Assert.Equal("savings", savings.Type);
        Assert.Equal("credit", credit.Type);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "To Delete",
            Type = "checking",
            Balance = 100.00M,
        };
        await _manager.SaveAsync(account);

        // Act
        var result = await _manager.DeleteAsync(accountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId.ToString(), result.Id);
        Assert.Equal("To Delete", result.Name);

        // Verify it was deleted
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.GetAsync(accountId));
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.DeleteAsync(invalidId));
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithNoAccounts_ReturnsEmptyList()
    {
        // Act
        var result = await _manager.ListAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListAsync_WithMultipleAccounts_ReturnsAllAccounts()
    {
        // Arrange
        await SeedAccount("Checking Account", "checking", 1000M);
        await SeedAccount("Savings Account", "savings", 5000M);
        await SeedAccount("Credit Card", "credit", -500M);

        // Act
        var result = await _manager.ListAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ListAsync_ReturnsAccountsWithCorrectData()
    {
        // Arrange
        var checkingId = Guid.NewGuid();
        var checking = new AccountModel
        {
            Id = checkingId.ToString(),
            Name = "My Checking",
            Type = "checking",
            Balance = 2500.75M,
        };
        await _manager.SaveAsync(checking);

        // Act
        var result = await _manager.ListAsync();

        // Assert
        var account = result.Single();
        Assert.Equal(checkingId.ToString(), account.Id);
        Assert.Equal("My Checking", account.Name);
        Assert.Equal("checking", account.Type);
        Assert.Equal(2500.75M, account.Balance);
    }

    [Fact]
    public async Task ListAsync_AfterDelete_DoesNotIncludeDeletedAccount()
    {
        // Arrange
        var account1Id = await SeedAccount("Account 1", "checking", 1000M);
        var account2Id = await SeedAccount("Account 2", "savings", 2000M);

        // Act - Delete one account
        await _manager.DeleteAsync(account1Id);
        var result = await _manager.ListAsync();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, a => a.Id == account2Id.ToString());
    }

    [Fact]
    public async Task ListAsync_AfterUpdate_ReturnsUpdatedData()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Original",
            Type = "checking",
            Balance = 100M,
        };
        await _manager.SaveAsync(account);

        // Act - Update and list
        var updated = new AccountModel
        {
            Id = account.Id,
            Name = "Updated",
            Type = account.Type,
            Balance = 200M,
        };
        await _manager.SaveAsync(updated);
        var result = await _manager.ListAsync();

        // Assert
        var retrieved = result.Single();
        Assert.Equal("Updated", retrieved.Name);
        Assert.Equal(200M, retrieved.Balance);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SaveAsync_WithZeroBalance_SavesCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Zero Balance",
            Type = "checking",
            Balance = 0.00M,
        };

        // Act
        await _manager.SaveAsync(account);
        var result = await _manager.GetAsync(accountId);

        // Assert
        Assert.Equal(0.00M, result.Balance);
    }

    [Fact]
    public async Task SaveAsync_WithNegativeBalance_SavesCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Credit Card",
            Type = "credit",
            Balance = -1500.50M,
        };

        // Act
        await _manager.SaveAsync(account);
        var result = await _manager.GetAsync(accountId);

        // Assert
        Assert.Equal(-1500.50M, result.Balance);
    }

    [Fact]
    public async Task SaveAsync_WithVeryLargeBalance_SavesCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new AccountModel
        {
            Id = accountId.ToString(),
            Name = "Investment Account",
            Type = "investment",
            Balance = 9999999.99M,
        };

        // Act
        await _manager.SaveAsync(account);
        var result = await _manager.GetAsync(accountId);

        // Assert
        Assert.Equal(9999999.99M, result.Balance);
    }

    #endregion
}
