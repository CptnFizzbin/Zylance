using Zylance.Desktop.Tests.Lib;
using Zylance.Desktop.Tests.Lib.Components.MenuBar;
using Zylance.Desktop.Tests.Lib.Screens;
using static Microsoft.Playwright.Assertions;

namespace Zylance.Desktop.Tests.E2E.MenuBar;

public class FileMenuTests : ZylanceDesktopTest
{
    [Fact]
    public async Task FileMenu_ShouldContainExpectedItems()
    {
        // Arrange
        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);

        // Act
        var fileMenu = await menuBar.OpenFileMenuAsync();

        // Assert
        await Expect(fileMenu.NewVault).ToBeVisibleAsync();
        await Expect(fileMenu.OpenVault).ToBeVisibleAsync();
        await Expect(fileMenu.CloseVault).ToBeVisibleAsync();
        await Expect(fileMenu.Exit).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NewVault_ShouldTriggerOnCreateFile()
    {
        // Arrange
        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);
        var fileMenu = await menuBar.OpenFileMenuAsync();
        var called = false;
        var tempVaultPath = Path.Combine(Harness.TempDataDir, "vault.zlv");
        Harness.FileProvider.OnCreateFile(() =>
        {
            called = true;
            return tempVaultPath;
        });

        // Act
        await fileMenu.NewVault.ClickAsync();
        // Wait for any UI update (e.g., Ledger visible)
        await Harness.Page.WaitForTimeoutAsync(500); // adjust as needed

        // Assert
        Assert.True(called, "OnCreateFile should be called when New Vault is clicked");
    }

    [Fact]
    public async Task OpenVault_ShouldTriggerOnSelectFile()
    {
        // Arrange
        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);
        var fileMenu = await menuBar.OpenFileMenuAsync();
        var called = false;
        var vaultPath = Harness.CopyFixtureToTempData("Vaults/EmptyVault.zlv", "vault.zlv");
        Harness.FileProvider.OnSelectFile(() =>
        {
            called = true;
            return vaultPath;
        });

        // Act
        await fileMenu.OpenVault.ClickAsync();
        await Harness.Page.WaitForTimeoutAsync(500);

        // Assert
        Assert.True(called, "OnSelectFile should be called when Open Vault is clicked");
    }

    [Fact]
    public async Task CloseVault_WhenNoVaultIsSelected_ShouldBeDisabled()
    {
        // Arrange
        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);

        // Act
        var fileMenu = await menuBar.OpenFileMenuAsync();

        // Assert
        await Expect(fileMenu.CloseVault).ToBeDisabledAsync();
    }

    [Fact]
    public async Task CloseVault_WhenAVaultIsSelected_ShouldCloseVault()
    {
        // Arrange
        var vaultPath = Harness.CopyFixtureToTempData("Vaults/EmptyVault.zlv", "vault.zlv");
        Harness.FileProvider.OnSelectFile(() => vaultPath);

        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);
        var fileMenu = await menuBar.OpenFileMenuAsync();
        await fileMenu.OpenVault.ClickAsync();
        await Harness.Page.WaitForTimeoutAsync(500);

        // Act
        fileMenu = await menuBar.OpenFileMenuAsync();
        await fileMenu.CloseVault.ClickAsync();

        // Assert
        var vaultSelectScreen = new VaultSelectScreen(Harness.Page);
        await vaultSelectScreen.WaitForActiveAsync();

        fileMenu = await menuBar.OpenFileMenuAsync();
        await Expect(fileMenu.CloseVault).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Exit_ShouldDisposeApplication()
    {
        // Arrange
        var menuBar = MenuBarComponent.GetFromPage(Harness.Page);
        var fileMenu = await menuBar.OpenFileMenuAsync();

        // Act
        await fileMenu.Exit.ClickAsync();
        // Poll for up to 2s for IsDisposed
        var disposed = false;
        for (var i = 0; i < 20; i++)
        {
            if (Harness.Desktop.IsDisposed)
            {
                disposed = true;
                break;
            }

            await Harness.Page.WaitForTimeoutAsync(100);
        }

        Assert.True(disposed, "Application should be disposed after Exit is clicked");
    }
}
