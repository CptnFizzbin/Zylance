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
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        // Clean up any temporary files created during tests
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        GC.SuppressFinalize(this);
    }

    private string CreateTempFilePath()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.zlv.sqlite");
        _tempFiles.Add(path);
        return path;
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
    public async Task FromFile_EmptyDatabase_ThrowsException()
    {
        // Arrange - create an empty SQLite database
        var filePath = CreateTempFilePath();

        using (var connection = new SqliteConnection($"Data Source={filePath}"))
        {
            await connection.OpenAsync();
            // Just open and close to create an empty database
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

        // Act - insert and read metadata
        using (var connection = new SqliteConnection($"Data Source={filePath}"))
        {
            await connection.OpenAsync();

            // Insert metadata
            using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = "INSERT INTO _zylance_ (Key, Value) VALUES ('version', '1.0.0')";
                await insertCommand.ExecuteNonQueryAsync();
            }

            // Read metadata
            using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Value FROM _zylance_ WHERE Key = 'version'";
            var value = await selectCommand.ExecuteScalarAsync();

            // Assert
            Assert.NotNull(value);
            Assert.Equal("1.0.0", value.ToString());
        }
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
