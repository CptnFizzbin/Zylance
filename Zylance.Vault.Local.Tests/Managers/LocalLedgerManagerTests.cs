using Zylance.Contract.Api.Ledger;
using Zylance.Contract.Models.Ledger;
using Zylance.Vault.Local.Context;
using Zylance.Vault.Local.Managers;

namespace Zylance.Vault.Local.Tests;

/// <summary>
///     Tests for LocalLedgerManager to ensure CRUD operations, filtering, and pagination work correctly.
/// </summary>
public class LocalLedgerManagerTests : IDisposable
{
    private readonly LocalVaultDbContext _context;
    private readonly LocalLedgerManager _manager;

    public LocalLedgerManagerTests()
    {
        _context = TestDbContextFactory.CreateContext();
        _manager = new LocalLedgerManager(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsLedgerEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entry = new LedgerEntryData
        {
            Id = entryId.ToString(),
            AccountId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payee = "Test Payee",
            Memo = "Test Memo",
            Amount = 100.50,
        };
        await _manager.SaveAsync(entry);

        // Act
        var result = await _manager.GetAsync(entryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entryId.ToString(), result.Id);
        Assert.Equal(accountId.ToString(), result.AccountId);
        Assert.Equal("Test Payee", result.Payee);
        Assert.Equal("Test Memo", result.Memo);
        Assert.Equal(100.50, result.Amount);
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
    public async Task SaveAsync_WithNewEntry_CreatesEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entry = new LedgerEntryData
        {
            Id = entryId.ToString(),
            AccountId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payee = "New Payee",
            Memo = "New Memo",
            Amount = 250.75,
        };

        // Act
        var result = await _manager.SaveAsync(entry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entryId.ToString(), result.Id);
        Assert.Equal("New Payee", result.Payee);
        Assert.Equal(250.75, result.Amount);

        // Verify it was persisted
        var retrieved = await _manager.GetAsync(entryId);
        Assert.Equal("New Payee", retrieved.Payee);
    }

    [Fact]
    public async Task SaveAsync_WithExistingEntry_UpdatesEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entry = new LedgerEntryData
        {
            Id = entryId.ToString(),
            AccountId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payee = "Original Payee",
            Memo = "Original Memo",
            Amount = 100.00,
        };
        await _manager.SaveAsync(entry);

        // Act - Update the entry
        entry.Payee = "Updated Payee";
        entry.Amount = 200.00;
        var result = await _manager.SaveAsync(entry);

        // Assert
        Assert.Equal("Updated Payee", result.Payee);
        Assert.Equal(200.00, result.Amount);

        // Verify it was updated
        var retrieved = await _manager.GetAsync(entryId);
        Assert.Equal("Updated Payee", retrieved.Payee);
        Assert.Equal(200.00, retrieved.Amount);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesEntry()
    {
        // Arrange
        var entryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entry = new LedgerEntryData
        {
            Id = entryId.ToString(),
            AccountId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payee = "To Delete",
            Memo = "Delete Me",
            Amount = 50.00,
        };
        await _manager.SaveAsync(entry);

        // Act
        var result = await _manager.DeleteAsync(entryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entryId.ToString(), result.Id);

        // Verify it was deleted
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.GetAsync(entryId));
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
    public async Task ListAsync_WithNoFilter_ReturnsAllEntries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntries(accountId, 5);

        // Act
        var result = await _manager.ListAsync(null);

        // Assert
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(5UL, result.TotalCount);
        Assert.True(result.IsLastPage);
        Assert.Empty(result.NextCursor);
    }

    [Fact]
    public async Task ListAsync_WithAccountFilter_ReturnsFilteredEntries()
    {
        // Arrange
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        await SeedLedgerEntries(account1, 3);
        await SeedLedgerEntries(account2, 2);

        var filter = new LedgerFilter { AccountId = account1.ToString() };

        // Act
        var result = await _manager.ListAsync(filter);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal(account1.ToString(), item.AccountId));
    }

    [Fact]
    public async Task ListAsync_WithTimestampFilter_ReturnsEntriesInRange()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var hourAgo = now - 3600000;
        var hourFromNow = now + 3600000;

        await SeedLedgerEntry(accountId, hourAgo - 1000); // Before range
        await SeedLedgerEntry(accountId, now); // In range
        await SeedLedgerEntry(accountId, hourFromNow + 1000); // After range

        var filter = new LedgerFilter { StartTimestamp = hourAgo, EndTimestamp = hourFromNow };

        // Act
        var result = await _manager.ListAsync(filter);

        // Assert
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ListAsync_WithPageSize_ReturnsCorrectNumberOfEntries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntries(accountId, 10);

        var filter = new LedgerFilter { PageSize = 3 };

        // Act
        var result = await _manager.ListAsync(filter);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(10UL, result.TotalCount);
        Assert.False(result.IsLastPage);
        Assert.NotEmpty(result.NextCursor);
    }

    [Fact]
    public async Task ListAsync_WithCursor_ReturnsNextPage()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntries(accountId, 10);

        var filter = new LedgerFilter { PageSize = 3 };

        // Act - Get first page
        var firstPage = await _manager.ListAsync(filter);
        Assert.Equal(3, firstPage.Items.Count);

        // Act - Get second page using cursor
        filter.Cursor = firstPage.NextCursor;
        var secondPage = await _manager.ListAsync(filter);

        // Assert
        Assert.Equal(3, secondPage.Items.Count);
        Assert.NotEqual(firstPage.Items[0].Id, secondPage.Items[0].Id);
    }

    [Fact]
    public async Task ListAsync_WithLargePageSize_CapsAtMaxPageSize()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntries(accountId, 150);

        var filter = new LedgerFilter { PageSize = 500 }; // Request more than max

        // Act
        var result = await _manager.ListAsync(filter);

        // Assert
        Assert.Equal(100, result.Items.Count); // Capped at MaxPageSize
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_WithPayeeMatch_ReturnsMatchingEntries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntry(accountId, payee: "Amazon Store");
        await SeedLedgerEntry(accountId, payee: "Walmart");
        await SeedLedgerEntry(accountId, payee: "Amazon Prime");

        // Act
        var result = await _manager.SearchAsync("Amazon", null);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Contains("Amazon", item.Payee));
    }

    [Fact]
    public async Task SearchAsync_WithMemoMatch_ReturnsMatchingEntries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntry(accountId, memo: "Groceries for party");
        await SeedLedgerEntry(accountId, memo: "Gas");
        await SeedLedgerEntry(accountId, memo: "Groceries for week");

        // Act
        var result = await _manager.SearchAsync("Groceries", null);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Contains("Groceries", item.Memo));
    }

    [Fact]
    public async Task SearchAsync_WithEmptySearchText_ReturnsAllEntries()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        await SeedLedgerEntries(accountId, 5);

        // Act
        var result = await _manager.SearchAsync("", null);

        // Assert
        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task SearchAsync_WithFilterAndSearchText_AppliesBoth()
    {
        // Arrange
        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        await SeedLedgerEntry(account1, payee: "Amazon Store");
        await SeedLedgerEntry(account1, payee: "Walmart");
        await SeedLedgerEntry(account2, payee: "Amazon Prime");

        var filter = new LedgerFilter { AccountId = account1.ToString() };

        // Act
        var result = await _manager.SearchAsync("Amazon", filter);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(account1.ToString(), result.Items[0].AccountId);
        Assert.Contains("Amazon", result.Items[0].Payee);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await SeedLedgerEntry(accountId, payee: $"Store {i}");

        var filter = new LedgerFilter { PageSize = 3 };

        // Act
        var result = await _manager.SearchAsync("Store", filter);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(10UL, result.TotalCount);
        Assert.False(result.IsLastPage);
    }

    #endregion

    #region Helper Methods

    private async Task SeedLedgerEntries(Guid accountId, int count)
    {
        for (var i = 0; i < count; i++)
            await SeedLedgerEntry(accountId);
    }

    private async Task SeedLedgerEntry(
        Guid accountId,
        long? timestamp = null,
        string payee = "Test Payee",
        string memo = "Test Memo"
    )
    {
        var entry = new LedgerEntryData
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = accountId.ToString(),
            Timestamp = timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Payee = payee,
            Memo = memo,
            Amount = 100.00,
        };
        await _manager.SaveAsync(entry);
    }

    #endregion
}
