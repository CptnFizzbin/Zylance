using Zylance.Desktop.Tests.Lib.Headless;
using Zylance.Desktop.Tests.Lib.PageContexts;

namespace Zylance.Desktop.Tests.Lib;

public class ZylanceDesktopTest : IAsyncLifetime
{
    protected ZylanceTestHarness Harness { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Harness = await ZylanceTestHarness.InitializeAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    public async ValueTask DisposeAsync()
    {
        await Harness.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected MenuBarContext MenuBar => new(Harness.Page);
}
