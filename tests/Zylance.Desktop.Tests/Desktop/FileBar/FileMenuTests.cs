using Microsoft.Playwright;
using Zylance.Desktop.Tests.Lib;
using static Microsoft.Playwright.Assertions;

namespace Zylance.Desktop.Tests.Desktop.FileBar;

public class FileMenuTests : ZylanceDesktopTest
{
    [Fact]
    public async Task NewVault_ShouldOpenNewVaultDialog()
    {
        await MenuBar.NewVaultAsync;

        var dialog = Harness.Page.GetByRole(
            AriaRole.Dialog,
            new PageGetByRoleOptions { Name = "Create New Vault" }
        );

        await Expect(dialog).ToBeVisibleAsync();
    }
}
