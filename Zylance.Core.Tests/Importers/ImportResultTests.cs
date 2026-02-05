using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Tests.Importers;

public class ImportResultTests
{
    [Fact]
    public void ImportResult_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true };

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public void ImportResult_Success_CanBeTrue()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true };

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void ImportResult_Success_CanBeFalse()
    {
        // Arrange & Act
        var result = new ImportResult { Success = false };

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void ImportResult_ErrorMessage_CanBeNull()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true, ErrorMessage = null };

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ImportResult_ErrorMessage_CanBeSet()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = new ImportResult { Success = false, ErrorMessage = errorMessage };

        // Assert
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void ImportResult_TransactionCount_DefaultsToZero()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true };

        // Assert
        Assert.Equal(0, result.TransactionCount);
    }

    [Fact]
    public void ImportResult_TransactionCount_CanBeSet()
    {
        // Arrange
        var count = 42;

        // Act
        var result = new ImportResult { Success = true, TransactionCount = count };

        // Assert
        Assert.Equal(count, result.TransactionCount);
    }

    [Fact]
    public void ImportResult_Warnings_CanBeNull()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true, Warnings = null };

        // Assert
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void ImportResult_Warnings_CanBeEmpty()
    {
        // Arrange & Act
        var result = new ImportResult { Success = true, Warnings = Array.Empty<string>() };

        // Assert
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ImportResult_Warnings_CanContainMultipleWarnings()
    {
        // Arrange
        var warnings = new[] { "Warning 1", "Warning 2", "Warning 3" };

        // Act
        var result = new ImportResult { Success = true, Warnings = warnings };

        // Assert
        Assert.NotNull(result.Warnings);
        Assert.Equal(3, result.Warnings.Count);
        Assert.Equal("Warning 1", result.Warnings[0]);
        Assert.Equal("Warning 2", result.Warnings[1]);
        Assert.Equal("Warning 3", result.Warnings[2]);
    }

    [Fact]
    public void ImportResult_Warnings_IsReadOnly()
    {
        // Arrange
        var warnings = new[] { "Warning 1" };

        // Act
        var result = new ImportResult { Success = true, Warnings = warnings };

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(result.Warnings);
    }

    [Fact]
    public void ImportResult_SuccessfulImport_WithTransactions()
    {
        // Arrange & Act
        var result = new ImportResult
        {
            Success = true,
            TransactionCount = 10,
            ErrorMessage = null,
            Warnings = null,
        };

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.TransactionCount);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void ImportResult_FailedImport_WithError()
    {
        // Arrange
        var errorMsg = "Invalid file format";

        // Act
        var result = new ImportResult
        {
            Success = false,
            TransactionCount = 0,
            ErrorMessage = errorMsg,
            Warnings = null,
        };

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.TransactionCount);
        Assert.Equal(errorMsg, result.ErrorMessage);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void ImportResult_SuccessfulImport_WithWarnings()
    {
        // Arrange
        var warnings = new[] { "Duplicate transaction found", "Unknown account type" };

        // Act
        var result = new ImportResult
        {
            Success = true,
            TransactionCount = 5,
            ErrorMessage = null,
            Warnings = warnings,
        };

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.TransactionCount);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Warnings);
        Assert.Equal(2, result.Warnings.Count);
    }

    [Fact]
    public void ImportResult_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var result1 = new ImportResult
        {
            Success = true,
            TransactionCount = 5,
            ErrorMessage = "Test",
        };

        var result2 = new ImportResult
        {
            Success = true,
            TransactionCount = 5,
            ErrorMessage = "Test",
        };

        // Act & Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ImportResult_IsRecord_DifferentValuesNotEqual()
    {
        // Arrange
        var result1 = new ImportResult { Success = true, TransactionCount = 5 };

        var result2 = new ImportResult { Success = true, TransactionCount = 10 };

        // Act & Assert
        Assert.NotEqual(result1, result2);
    }
}
