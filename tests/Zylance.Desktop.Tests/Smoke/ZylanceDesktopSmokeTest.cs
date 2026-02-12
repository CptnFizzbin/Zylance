using Microsoft.Playwright;
using Zylance.Desktop.Tests.Lib.Headless;

namespace Zylance.Desktop.Tests.Smoke;

public class ZylanceDesktopSmokeTest
{
    [Fact]
    public async Task ZylanceDesktop_LaunchesAndDisplaysAppName()
    {
        var harness = await ZylanceTestHarness.InitializeAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(harness.Page);

        await Assertions.Expect(harness.Page.Locator("text=Zylance")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ZylanceDesktop_CreatesVaultAndShowsLedger()
    {
        var harness = await ZylanceTestHarness.InitializeAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(harness.Page);

        var tempVaultPath = Path.Combine(harness.TempDataDir, $"test_{Guid.NewGuid()}.zlv.sqlite");
        harness.FileProvider.OnCreateFile = (_, _, _) => Task.FromResult(tempVaultPath);

        var createButton = harness.Page.Locator("button:has-text(\"Create New Vault\")");
        await createButton.ClickAsync();

        await Assertions.Expect(harness.Page.Locator("text=Ledger")).ToBeVisibleAsync();
    }
}
