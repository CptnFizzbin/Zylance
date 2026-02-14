using Microsoft.Playwright;
using Zylance.Core;
using Zylance.Desktop.Config;

namespace Zylance.Desktop.Tests.TestUtils;

public record ZylanceTestHarness : IAsyncDisposable
{
    private string FixturesDir { get; } =
        Path.Combine(FindSolutionRoot(), "tests", "Zylance.Desktop.Tests", "Fixtures");

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

    /// <summary>
    ///     The directory where the app stores temporary data during tests.
    ///     This is a unique temp directory created for each test run, and is
    ///     automatically cleaned up when the test harness is disposed.
    /// </summary>
    public string TempDataDir => Desktop.Config.TmpDataPath;

    /// <summary>
    ///     The directory where the app stores its data during tests. This is a
    ///     unique temp directory created for each test run, and is automatically
    ///     cleaned up when the test harness is disposed.
    /// </summary>
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
        int? uiPort = null,
        int? wsPort = null,
        int appReadyTimeoutMs = 10000,
        CancellationToken cancellationToken = default
    )
    {
        var solutionRoot = FindSolutionRoot();
        var uiRootPath = Path.Combine(solutionRoot, "src", "Zylance.UI", "dist");
        if (!Directory.Exists(uiRootPath))
            throw new DirectoryNotFoundException($"UI root path does not exist: {uiRootPath}");

        var appDataDir = Path.Combine(Path.GetTempPath(), "Zylance.AppData", Guid.NewGuid().ToString());
        Directory.CreateDirectory(appDataDir);

        var tempDataDir = Path.Combine(Path.GetTempPath(), "Zylance.Temp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDataDir);

        var fileProvider = new HeadlessFileProvider(appDataDir, tempDataDir);

        var config = new ZylanceDesktopConfig(uiPort, wsPort)
        {
            Headless = true,
            UiServerEnabled = true,
            UiRootPath = uiRootPath,
            AppDataPath = appDataDir,
            TmpDataPath = tempDataDir,
        };

        var desktop = new ZylanceDesktop(config, fileProvider: fileProvider);
        desktop.Start();

        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
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

    /// <summary>
    ///     Copies a fixture file from the Fixtures directory to a target path.
    /// </summary>
    /// <param name="relativeFixturePath">
    ///     Path relative to Fixtures/
    ///     (e.g. "Vaults/EmptyVault.zlv")
    /// </param>
    /// <param name="relativeDestinationPath">
    ///     Path relative to the AppData/
    ///     (e.g. "user-documents/vault.zlv")
    /// </param>
    public string CopyFixtureToAppData(string relativeFixturePath, string relativeDestinationPath)
    {
        var fixturePath = Path.Combine(FixturesDir, relativeFixturePath);
        var destinationPath = Path.Combine(AppDataDir, relativeDestinationPath);

        if (!File.Exists(fixturePath))
            throw new FileNotFoundException($"Fixture file not found: {fixturePath}");

        File.Copy(fixturePath, destinationPath, true);

        return destinationPath;
    }

    /// <summary>
    ///     Copies a fixture file from the Fixtures directory to a target path.
    /// </summary>
    /// <param name="relativeFixturePath">
    ///     Path relative to Fixtures/
    ///     (e.g. "Vaults/EmptyVault.zlv")
    /// </param>
    /// <param name="relativeDestinationPath">
    ///     Path relative to the TempData/
    ///     (e.g. "user-documents/vault.zlv")
    /// </param>
    public string CopyFixtureToTempData(string relativeFixturePath, string relativeDestinationPath)
    {
        var fixturePath = Path.Combine(FixturesDir, relativeFixturePath);
        var destinationPath = Path.Combine(TempDataDir, relativeDestinationPath);

        if (!File.Exists(fixturePath))
            throw new FileNotFoundException($"Fixture file not found: {fixturePath}");

        File.Copy(fixturePath, destinationPath, true);

        return destinationPath;
    }
}
