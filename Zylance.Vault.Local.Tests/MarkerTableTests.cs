using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Zylance.Vault.Local.Context;

namespace Zylance.Vault.Local.Tests;

/// <summary>
///     Tests for the _zylance_ marker table functionality.
///     Validates that the marker table is created and checked correctly.
/// </summary>
public class MarkerTableTests : IDisposable
{
    private readonly string _tempDirectory;

    public MarkerTableTests()
    {
        // Create a unique temporary directory for all test files
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"zylance.test.{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private string CreateTempFilePath(string? fileName = null, bool includeUuid = true)
    {
        if (fileName is null)
        {
            fileName = includeUuid ? $"test_{Guid.NewGuid()}.zlv.sqlite" : "test.zlv.sqlite";
        }
        else if (includeUuid)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            fileName = $"{fileNameWithoutExt}_{Guid.NewGuid()}{extension}";
        }

        return Path.Combine(_tempDirectory, fileName);
    }

    private async Task<bool> TableExistsAsync(string filePath, string tableName)
    {
        using var connection = new SqliteConnection($"Data Source={filePath}");
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
        command.Parameters.AddWithValue("@tableName", tableName);
        var result = await command.ExecuteScalarAsync();
        return result is not null && Convert.ToInt32(result) > 0;
    }

    [Fact]
    public async Task FromFile_NewDatabase_CreatesMarkerTable()
    {
        // Arrange
        var filePath = CreateTempFilePath();

        // Act
        var vault = await LocalVault.FromFile(filePath);

        // Assert
        Assert.NotNull(vault);

        // Verify the marker table exists
        var tableExists = await TableExistsAsync(filePath, "_zylance_");
        Assert.True(tableExists);
    }

    [Fact]
    public async Task FromFile_ExistingZylanceDatabase_OpensSuccessfully()
    {
        // Arrange - create a Zylance database
        var filePath = CreateTempFilePath();
        var initialVault = await LocalVault.FromFile(filePath);

        // Act - open the existing database
        var vault = await LocalVault.FromFile(filePath);

        // Assert
        Assert.NotNull(vault);
    }

    [Fact]
    public async Task FromFile_NonZylanceDatabase_ThrowsException()
    {
        // Arrange - create a SQLite database without the marker table
        var filePath = CreateTempFilePath();

        using (var connection = new SqliteConnection($"Data Source={filePath}"))
        {
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE SomeOtherTable (Id INTEGER PRIMARY KEY, Name TEXT)";
            await command.ExecuteNonQueryAsync();
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NonZylanceDatabaseException>(() => LocalVault.FromFile(filePath));

        Assert.Contains("not a Zylance vault", exception.Message);
        Assert.Contains(filePath, exception.Message);
    }

    [Fact]
    public async Task FromFile_NonZylanceDatabase_DoesNotRunMigrations()
    {
        // Arrange - create a SQLite database without the marker table
        var filePath = CreateTempFilePath();

        using (var connection = new SqliteConnection($"Data Source={filePath}"))
        {
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE SomeOtherTable (Id INTEGER PRIMARY KEY, Name TEXT)";
            await command.ExecuteNonQueryAsync();
        }

        // Act & Assert - expect exception
        await Assert.ThrowsAsync<NonZylanceDatabaseException>(() => LocalVault.FromFile(filePath));

        // Verify that migrations were not run - check that Zylance tables don't exist
        var accountsTableExists = await TableExistsAsync(filePath, "Accounts");
        var ledgerEntriesTableExists = await TableExistsAsync(filePath, "LedgerEntries");
        var markerTableExists = await TableExistsAsync(filePath, "_zylance_");

        Assert.False(accountsTableExists, "Accounts table should not exist");
        Assert.False(ledgerEntriesTableExists, "LedgerEntries table should not exist");
        Assert.False(markerTableExists, "_zylance_ table should not exist");
    }

    [Fact]
    public async Task FromFile_EmptyDatabase_ThrowsException()
    {
        // Arrange - create an empty SQLite database by opening and closing a connection
        var filePath = CreateTempFilePath();

        using (var connection = new SqliteConnection($"Data Source={filePath}"))
        {
            await connection.OpenAsync();
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NonZylanceDatabaseException>(() => LocalVault.FromFile(filePath));

        Assert.Contains("not a Zylance vault", exception.Message);
    }

    [Fact]
    public async Task MarkerTable_CanStoreMetadata()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        var vault = await LocalVault.FromFile(filePath);

        // Act - set and get metadata using the Metadata API
        await vault.Metadata.SetAsync("version", "1.0.0");
        var value = await vault.Metadata.GetAsync("version");

        // Assert
        Assert.NotNull(value);
        Assert.Equal("1.0.0", value);
    }

    [Fact]
    public async Task Metadata_GetAsync_NonExistentKey_ReturnsNull()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        var vault = await LocalVault.FromFile(filePath);

        // Act
        var value = await vault.Metadata.GetAsync("nonexistent");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public async Task Metadata_SetAsync_UpdatesExistingValue()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        var vault = await LocalVault.FromFile(filePath);
        await vault.Metadata.SetAsync("version", "1.0.0");

        // Act - update the value
        await vault.Metadata.SetAsync("version", "2.0.0");
        var value = await vault.Metadata.GetAsync("version");

        // Assert
        Assert.Equal("2.0.0", value);
    }

    [Fact]
    public async Task MarkerTable_HasCorrectSchema()
    {
        // Arrange
        var filePath = CreateTempFilePath();
        await LocalVault.FromFile(filePath);

        // Act - query table schema
        using var connection = new SqliteConnection($"Data Source={filePath}");
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(_zylance_)";

        var columns = new List<(string Name, string Type, bool NotNull)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(
                (
                    reader.GetString(1), // name
                    reader.GetString(2), // type
                    reader.GetInt32(3) == 1 // notnull
                )
            );
        }

        // Assert
        Assert.Equal(2, columns.Count);

        var keyColumn = columns.First(c => c.Name == "Key");
        Assert.Equal("TEXT", keyColumn.Type);
        Assert.True(keyColumn.NotNull);

        var valueColumn = columns.First(c => c.Name == "Value");
        Assert.Equal("TEXT", valueColumn.Type);
        Assert.True(valueColumn.NotNull);
    }
}
