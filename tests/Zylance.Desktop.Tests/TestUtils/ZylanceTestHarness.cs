using Microsoft.Playwright;
using Zylance.Core;
using Zylance.Desktop.Config;

namespace Zylance.Desktop.Tests.TestUtils;

public record ZylanceTestHarness : IAsyncDisposable
{
    public required ZylanceDesktop Desktop { get; init; }
    public required HeadlessFileProvider FileProvider { get; init; }
    public required IBrowser Browser { get; init; }
    public required IPage Page { get; init; }
    public required IPlaywright Playwright { get; init; }
    public required IBrowserContext BrowserContext { get; init; }
    public int UiPort => Desktop.Config.UiPort;
    public string UiUrl => Desktop.Config.UiServerUrl;
    public int WsPort => Desktop.Config.WsPort;

    public string WsUrl => Desktop.Config.WebSocketUrl;
    public string TempDataDir => Desktop.Config.TmpDataPath;
    public string AppDataDir => Desktop.Config.AppDataPath;
    public ZylanceCore Zylance => Desktop.ZylanceCore;

    public async ValueTask DisposeAsync()
    {
        await Desktop.DisposeAsync();
        await Browser.DisposeAsync();
        await BrowserContext.DisposeAsync();

        FileProvider.Dispose();
        Playwright.Dispose();

        GC.SuppressFinalize(this);
    }

    public static async Task<ZylanceTestHarness> InitializeAsync(
        int uiPort = 8123,
        int wsPort = 8124,
        int appReadyTimeoutMs = 10000,
        CancellationToken cancellationToken = default
    )
    {
        var solutionRoot = FindSolutionRoot();
        var uiRootPath = Path.Combine(solutionRoot, "Zylance.UI", "dist");
        if (!Directory.Exists(uiRootPath))
            throw new DirectoryNotFoundException($"UI root path does not exist: {uiRootPath}");

        var appDataDir = Path.Combine(Path.GetTempPath(), "Zylance.AppData", Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataDir);

        var tempDataDir = Path.Combine(Path.GetTempPath(), "Zylance.Temp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDataDir);

        var fileProvider = new HeadlessFileProvider(appDataDir, tempDataDir);

        var config = new ZylanceDesktopConfig
        {
            Headless = true,
            UiServerEnabled = true,
            UiPort = uiPort,
            WsPort = wsPort,
            UiRootPath = uiRootPath,
            AppDataPath = appDataDir,
            TmpDataPath = tempDataDir,
        };
        var desktop = new ZylanceDesktop(config, fileProvider: fileProvider);
        desktop.Start();

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var browserContext = await browser.NewContextAsync();
        browserContext.SetDefaultTimeout((float)TimeSpan.FromSeconds(10).TotalMilliseconds);
        var page = await browserContext.NewPageAsync();

        var harness = new ZylanceTestHarness
        {
            Desktop = desktop,
            FileProvider = fileProvider,
            Browser = browser,
            Page = page,
            Playwright = playwright,
            BrowserContext = browserContext,
        };

        await harness.Page.GotoAsync(harness.UiUrl);
        await harness.WaitForAppReadyAsync(appReadyTimeoutMs, cancellationToken);
        return harness;
    }

    private async Task WaitForAppReadyAsync(int timeoutMs = 10000, CancellationToken cancellationToken = default)
    {
        var readyTcs = new TaskCompletionSource<bool>();

        if (Page is null)
            throw new InvalidOperationException("Page is not initialized.");

        void ConsoleHandler(object? sender, IConsoleMessage msg)
        {
            if (msg.Text.Contains("Zylance Loaded"))
                readyTcs.TrySetResult(true);
        }

        Page.Console += ConsoleHandler;

        var completedTask = await Task.WhenAny(readyTcs.Task, Task.Delay(timeoutMs, cancellationToken));
        Page.Console -= ConsoleHandler;

        if (completedTask != readyTcs.Task)
            throw new TimeoutException($"App did not signal ready within {timeoutMs}ms");

        await readyTcs.Task;
    }

    private static string FindSolutionRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && dir.Length > 3)
        {
            if (File.Exists(Path.Combine(dir, "Zylance.sln")))
                return dir;

            dir = Path.GetDirectoryName(dir);
        }

        throw new DirectoryNotFoundException("Could not find Zylance.sln in any parent directory.");
    }
}
