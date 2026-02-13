using Microsoft.Playwright;
using Zylance.Desktop.Tests.Lib;

namespace Zylance.Desktop.Tests.E2E.Smoke;

public class ZylanceDesktopSmokeTest : ZylanceDesktopTest
{
    [Fact]
    public async Task ZylanceDesktop_LaunchesAndDisplaysAppName()
    {
        Assert.NotNull(Harness.Page);
        await Assertions.Expect(Harness.Page.Locator("text=Zylance")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ZylanceDesktop_CreatesVaultAndShowsLedger()
    {
        Assert.NotNull(Harness.Page);

        var tempVaultPath = Path.Combine(Harness.TempDataDir, "test.zlv");
        Harness.FileProvider.OnCreateFile(() => tempVaultPath);

        var createButton = Harness.Page.Locator("button:has-text(\"Create New Vault\")");
        await createButton.ClickAsync();

        await Assertions.Expect(Harness.Page.Locator("text=Ledger")).ToBeVisibleAsync();
    }
}
