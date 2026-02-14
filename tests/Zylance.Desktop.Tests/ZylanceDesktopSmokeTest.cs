using Microsoft.Playwright;
using Zylance.Desktop.Tests.TestUtils;

namespace Zylance.Desktop.Tests;

public class ZylanceDesktopSmokeTest
{
    [Fact]
    public async Task ZylanceDesktop_LaunchesAndDisplaysAppName()
    {
        await using var harness = await ZylanceTestHarness.InitializeAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(harness.Page);

        await Assertions.Expect(harness.Page.Locator("text=Zylance")).ToBeVisibleAsync();
    }
}
