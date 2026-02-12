using Microsoft.Playwright;
using Zylance.Desktop.Tests.Lib;
using Zylance.Desktop.Tests.Lib.Headless;

namespace Zylance.Desktop.Tests.Smoke;

/// <summary>
///     End-to-end test covering the full vault lifecycle from creation through usage.
/// </summary>
public class VaultLifecycleE2ETest : ZylanceDesktopTest
{
    [Fact]
    public async Task FullVaultLifecycle_CreateOpenCloseReopen_WorksEndToEnd()
    {
        Assert.NotNull(Harness);
        var page = Harness.Page;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: App launches and displays welcome screen
        await Assertions.Expect(page.Locator("text=Zylance")).ToBeVisibleAsync();
        await Assertions
            .Expect(page.Locator("text=Your Personal Finance Vault"))
            .ToBeVisibleAsync();

        // Step 2: Create a new vault
        var tempVaultPath = Path.Combine(
            Harness.TempDataDir,
            $"e2e_test_{Guid.NewGuid()}.zlv.sqlite"
        );
        Harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(tempVaultPath);

        var createButton = page.Locator("button:has-text(\"Create New Vault\")");
        await Assertions.Expect(createButton).ToBeVisibleAsync();
        await createButton.ClickAsync();

        // Step 3: Wait for vault creation and navigation to unlock screen
        // The app should navigate to unlock-vault route after creation
        await page.WaitForURLAsync("**/locked/unlock-vault", new PageWaitForURLOptions
        {
            Timeout = 5000
        });

        // Step 4: Verify we're on the unlock screen (would normally require password)
        // For now, the vault is auto-unlocked since Locked property returns false
        await page.WaitForURLAsync("**/vault/**", new PageWaitForURLOptions
        {
            Timeout = 5000
        });

        // Step 5: Verify ledger view is displayed
        await Assertions.Expect(page.Locator("text=Ledger")).ToBeVisibleAsync();

        // Step 6: Verify Accounts panel is visible
        await Assertions.Expect(page.Locator("text=Accounts")).ToBeVisibleAsync();

        // Step 7: Verify the vault file was created
        Assert.True(File.Exists(tempVaultPath), $"Vault file should exist at {tempVaultPath}");

        // Step 8: Verify vault file is a valid SQLite database with marker table
        var vault = await Zylance.Vault.Local.LocalVault.FromFile(tempVaultPath, cancellationToken);
        Assert.NotNull(vault);
        Assert.NotEqual(Guid.Empty, vault.VaultId);

        // Verify we can interact with the vault
        await vault.Metadata.SetAsync("test_key", "test_value", cancellationToken);
        var value = await vault.Metadata.GetAsync("test_key", cancellationToken);
        Assert.Equal("test_value", value);

        // Clear connection pools to release the vault
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Step 9: Simulate closing and reopening vault
        // Navigate back to vault selection screen
        await page.GotoAsync($"{Harness.UiUrl}/locked/select-vault");
        await Assertions.Expect(page.Locator("text=Select Your Vault")).ToBeVisibleAsync();

        // Step 10: Reopen the existing vault
        Harness.FileProvider.OnSelectFile = (_, _, _) => Task.FromResult(tempVaultPath);

        var openButton = page.Locator("button:has-text(\"Open Existing Vault\")");
        await Assertions.Expect(openButton).ToBeVisibleAsync();
        await openButton.ClickAsync();

        // Step 11: Navigate through unlock screen again
        await page.WaitForURLAsync("**/vault/**", new PageWaitForURLOptions
        {
            Timeout = 5000
        });

        // Step 12: Verify ledger is displayed again
        await Assertions.Expect(page.Locator("text=Ledger")).ToBeVisibleAsync();

        // Step 13: Verify data persisted
        var reopenedVaultForVerification = await Zylance.Vault.Local.LocalVault.FromFile(
            tempVaultPath,
            cancellationToken
        );
        var persistedValue = await reopenedVaultForVerification.Metadata.GetAsync(
            "test_key",
            cancellationToken
        );
        Assert.Equal("test_value", persistedValue);

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    }

    [Fact]
    public async Task CreateVault_UserCancels_StaysOnSelectScreen()
    {
        Assert.NotNull(Harness);
        var page = Harness.Page;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Setup: Return empty path to simulate cancellation
        Harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(string.Empty);

        // Act: Click create vault button
        var createButton = page.Locator("button:has-text(\"Create New Vault\")");
        await createButton.ClickAsync();

        // Wait a moment for any potential navigation
        await Task.Delay(500, cancellationToken);

        // Assert: Should still be on select vault screen
        await Assertions.Expect(page.Locator("text=Select Your Vault")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task OpenVault_UserCancels_StaysOnSelectScreen()
    {
        Assert.NotNull(Harness);
        var page = Harness.Page;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Setup: Return empty path to simulate cancellation
        Harness.FileProvider.OnSelectFile = (_, _, _) => Task.FromResult(string.Empty);

        // Act: Click open vault button
        var openButton = page.Locator("button:has-text(\"Open Existing Vault\")");
        await openButton.ClickAsync();

        // Wait a moment for any potential navigation
        await Task.Delay(500, cancellationToken);

        // Assert: Should still be on select vault screen
        await Assertions.Expect(page.Locator("text=Select Your Vault")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreateVault_WithMetadata_PersistsAcrossReopen()
    {
        Assert.NotNull(Harness);
        var page = Harness.Page;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Step 1: Create a new vault
        var tempVaultPath = Path.Combine(
            Harness.TempDataDir,
            $"metadata_test_{Guid.NewGuid()}.zlv.sqlite"
        );
        Harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(tempVaultPath);

        var createButton = page.Locator("button:has-text(\"Create New Vault\")");
        await createButton.ClickAsync();

        // Step 2: Wait for vault to be created and opened
        await page.WaitForURLAsync("**/vault/**", new PageWaitForURLOptions
        {
            Timeout = 5000
        });

        // Step 3: Write test data directly to vault
        var vaultForWrite = await Zylance.Vault.Local.LocalVault.FromFile(
            tempVaultPath,
            cancellationToken
        );
        await vaultForWrite.Metadata.SetAsync("app_version", "1.0.0", cancellationToken);
        await vaultForWrite.Metadata.SetAsync("user_preference", "dark_mode", cancellationToken);
        await vaultForWrite.Metadata.SetAsync(
            "last_opened",
            DateTime.UtcNow.ToString("O"),
            cancellationToken
        );

        // Clear connection pools to ensure clean reopen
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Step 4: Reopen vault and verify all metadata persisted
        var vaultForRead = await Zylance.Vault.Local.LocalVault.FromFile(
            tempVaultPath,
            cancellationToken
        );
        var appVersion = await vaultForRead.Metadata.GetAsync("app_version", cancellationToken);
        var userPreference = await vaultForRead.Metadata.GetAsync(
            "user_preference",
            cancellationToken
        );
        var lastOpened = await vaultForRead.Metadata.GetAsync("last_opened", cancellationToken);

        Assert.Equal("1.0.0", appVersion);
        Assert.Equal("dark_mode", userPreference);
        Assert.NotNull(lastOpened);
        Assert.True(DateTime.TryParse(lastOpened, out _));

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    }

    [Fact]
    public async Task CreateMultipleVaults_EachHasIndependentData()
    {
        Assert.NotNull(Harness);
        var page = Harness.Page;
        var cancellationToken = TestContext.Current.CancellationToken;

        // Create first vault
        var vault1Path = Path.Combine(Harness.TempDataDir, $"vault1_{Guid.NewGuid()}.zlv.sqlite");
        Harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(vault1Path);

        var createButton = page.Locator("button:has-text(\"Create New Vault\")");
        await createButton.ClickAsync();
        await page.WaitForURLAsync("**/vault/**", new PageWaitForURLOptions { Timeout = 5000 });

        // Add data to first vault
        var vault1 = await Zylance.Vault.Local.LocalVault.FromFile(vault1Path, cancellationToken);
        await vault1.Metadata.SetAsync("vault_name", "Personal", cancellationToken);

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Navigate back to select screen
        await page.GotoAsync($"{Harness.UiUrl}/locked/select-vault");

        // Create second vault
        var vault2Path = Path.Combine(Harness.TempDataDir, $"vault2_{Guid.NewGuid()}.zlv.sqlite");
        Harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(vault2Path);

        await createButton.ClickAsync();
        await page.WaitForURLAsync("**/vault/**", new PageWaitForURLOptions { Timeout = 5000 });

        // Add different data to second vault
        var vault2 = await Zylance.Vault.Local.LocalVault.FromFile(vault2Path, cancellationToken);
        await vault2.Metadata.SetAsync("vault_name", "Business", cancellationToken);

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Verify both vaults have independent data
        var vault1ForVerify = await Zylance.Vault.Local.LocalVault.FromFile(
            vault1Path,
            cancellationToken
        );
        var name1 = await vault1ForVerify.Metadata.GetAsync("vault_name", cancellationToken);
        Assert.Equal("Personal", name1);

        var vault2ForVerify = await Zylance.Vault.Local.LocalVault.FromFile(
            vault2Path,
            cancellationToken
        );
        var name2 = await vault2ForVerify.Metadata.GetAsync("vault_name", cancellationToken);
        Assert.Equal("Business", name2);

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
    }
}
