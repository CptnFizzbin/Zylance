# ADR-011: Headless End-to-End Testing for UI

## Context

As Zylance grows, we need comprehensive end-to-end (E2E) testing that validates
the entire application stack—from the React UI through Protocol Buffers
communication to the C# backend and database layer. This is particularly
critical for:

1. **Integration validation**: Ensuring UI, transport layer, Gateway,
   controllers, and vault providers work together correctly
2. **Regression prevention**: Catching breaking changes across the stack before
   they reach production
3. **Cross-platform confidence**: Verifying that the single UI codebase works
   correctly on all platforms
4. **CI/CD pipeline**: Running automated E2E tests in a headless environment
   without a display

Currently, testing is fragmented:

- **Unit tests**: Exist for Core (xUnit), Vault implementations, and some UI
  components (Vitest)
- **Integration tests**: Limited, mostly test individual layers in isolation
- **E2E tests**: None—manual testing is required to validate full stack
  interaction
- **CI limitations**: Cannot run visual UI tests in CI because there's no
  headless mode

This creates several problems:

1. **Slow feedback loop**: Bugs in integration between layers are only caught
   during manual testing
2. **Manual testing burden**: Every change requires manual verification of UI
   functionality
3. **CI gaps**: Cannot automatically verify that UI changes work with backend
   changes
4. **Platform coverage**: Difficult to test all platform-specific code paths
5. **Regression risk**: No automated way to catch regressions in critical user
   flows

Options considered for headless E2E testing:

- **Playwright/Selenium against external browser**: Requires launching the
  Desktop app separately, complex coordination
- **Playwright in Photino.NET WebView**: Not feasible—Photino doesn't expose
  automation APIs
- **Chromium DevTools Protocol (CDP) in headless mode**: Complex to integrate
  with Photino
- **Headless browser with custom transport**: Replace Photino with headless
  browser for testing
- **WebDriver with Photino**: Photino doesn't support WebDriver protocol
- **Test-specific headless mode flag**: Add `--headless` flag that uses headless
  Chromium instead of Photino

We need a solution that:

1. Runs in CI environments without a display
2. Works with existing test frameworks (xUnit, Playwright/Cypress)
3. Exercises the full stack (UI, transport, backend)
4. Minimizes changes to production code
5. Provides debugging capabilities for failed tests

## Implementation

**Status**: Implemented (see `Zylance.Desktop.Tests/Headless/`)

## Decision

Introduce a **`--headless` flag** to `Zylance.Desktop` that runs the application
with a **headless Chromium browser** instead of Photino.NET's native WebView,
enabling automated E2E testing in CI environments.

The implementation:

### 1. Headless Mode Architecture

When `--headless` is enabled, replace the Photino stack with headless browser
infrastructure:

```
Normal Mode (Photino):
  PhotinoWindow → WebView → UI → PhotinoTransport → Gateway → Controllers

Headless Mode:
  PuppeteerSharp/Playwright → Headless Chromium → UI → WebSocketTransport → Gateway → Controllers
```

Key differences:

- **Window**: Headless Chromium instead of native WebView
- **Transport**: WebSocket-based transport instead of Photino's
  `SendWebMessage`/`RegisterWebMessageReceivedHandler`
- **Process model**: Browser runs in separate process, controlled via CDP (
  Chrome DevTools Protocol)
- **Automation API**: Tests can control browser via Playwright/Puppeteer

### 2. New Transport Implementation

Create `WebSocketTransport` for headless mode:

```csharp
/// <summary>
/// WebSocket-based transport for headless browser communication.
/// Used during testing when --headless flag is enabled.
/// </summary>
public class WebSocketTransport : ITransport
{
    private readonly WebSocket _webSocket;
    private Action<string>? _messageHandler;
    
    public WebSocketTransport(int port = 8080)
    {
        // Start WebSocket server for UI to connect to
        // This replaces Photino's web message API
    }
    
    public void Send(string message)
    {
        _webSocket.Send(message);
    }
    
    public void Receive(Action<string> callback)
    {
        _messageHandler = callback;
    }
}
```

This provides the same `ITransport` interface as `PhotinoTransport`, maintaining
compatibility with the rest of the application.

### 3. Headless Browser Launcher

Create `HeadlessBrowserLauncher` to manage the headless Chromium instance:

```csharp
public class HeadlessBrowserLauncher
{
    private IBrowser? _browser;
    private IPage? _page;
    
    public async Task<IPage> LaunchAsync(string appUrl, bool devTools = false)
    {
        // Launch headless Chromium using PuppeteerSharp or Playwright
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });
        
        _page = await _browser.NewPageAsync();
        await _page.GoToAsync(appUrl);
        
        return _page;
    }
    
    public async Task CloseAsync()
    {
        if (_page is not null) await _page.CloseAsync();
        if (_browser is not null) await _browser.CloseAsync();
    }
}
```

### 4. Modified Program.cs

Update `Program.cs` to detect `--headless` flag and use appropriate stack:

```csharp
private static async Task Main(string[] args)
{
    var isHeadless = args.Contains("--headless");
    
    if (isHeadless)
    {
        await RunHeadlessAsync();
    }
    else
    {
        RunWithPhotino();
    }
}

private static async Task RunHeadlessAsync()
{
    var appUrl = GetServerUrl();
    
    // Use WebSocket transport for headless mode
    var transport = new WebSocketTransport(port: 8080);
    var fileProvider = new DesktopFileProvider(null); // null for headless
    var vaultProvider = new DesktopVaultProvider(fileProvider);
    
    _ = new Core.Zylance(transport, fileProvider, vaultProvider);
    
    // Launch headless browser
    var launcher = new HeadlessBrowserLauncher();
    var page = await launcher.LaunchAsync(appUrl.ToString());
    
    // Keep running until explicitly stopped (for testing)
    await Task.Delay(-1);
}

private static void RunWithPhotino()
{
    // Existing Photino implementation
    // ...
}
```

### 5. UI Transport Adapter

Update `Zylance.UI` to support WebSocket transport in addition to Photino's web
message API:

```typescript
// Detect transport type based on environment
const transport: ITransport = window.Photino
  ? new PhotinoTransport()
  : new WebSocketTransport("ws://localhost:8080");

const zylanceClient = new ZylanceClient(transport);
```

This allows the UI to work with either transport seamlessly.

### 6. E2E Test Harness

The E2E test harness is implemented as `ZylanceTestHarness` in
`Zylance.Desktop.Tests/Headless/`. It provides:

- Automated setup/teardown of a real `ZylanceDesktop` instance in headless mode
- Playwright integration for browser automation (`Page`, `Browser`,
  `BrowserContext`)
- Management of temporary app data and file directories for test isolation
- A `HeadlessFileProvider` that allows tests to control file selection/creation
  dialogs via callbacks
- Automatic waiting for the app to be ready (by listening for a
  `"Zylance Loaded"` console message)

#### Example: ZylanceTestHarness API

```csharp
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
    // ... DisposeAsync, InitializeAsync, WaitForAppReadyAsync ...
}
```

#### Example: HeadlessFileProvider

```csharp
public class HeadlessFileProvider : LocalFileProvider
{
    public CreateFileHandler OnCreateFile = ...;
    public SelectFileHandler OnSelectFile = ...;
    public override async Task<FileRef> SelectFile(...) { ... }
    public override async Task<FileRef> CreateFile(...) { ... }
}
```

Tests set `OnCreateFile` and `OnSelectFile` to control file dialogs during E2E
flows.

#### Example: E2E Test Using the Harness

```csharp
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
```

#### Example: App Ready Wait

The harness waits for a `"Zylance Loaded"` console message before returning from
`InitializeAsync`, ensuring the app is ready for interaction:

```csharp
private async Task WaitForAppReadyAsync(int timeoutMs = 10000, CancellationToken cancellationToken = default)
{
    var readyTcs = new TaskCompletionSource<bool>();
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
```

### 7. E2E Test Patterns

- All E2E tests should use `ZylanceTestHarness.InitializeAsync()` for setup and
  teardown.
- File dialogs are simulated by setting callbacks on the harness's
  `HeadlessFileProvider`.
- Playwright's `Page` API is used for UI automation and assertions.
- Temp directories are unique per test for isolation.
- The harness ensures proper cleanup of all resources.

## Consequences

### Positive

- **CI/CD compatibility**: Tests run in headless environments without a display
- **Full stack coverage**: E2E tests validate entire application flow from UI to
  database
- **Debuggable**: Failed tests can be investigated with screenshots, traces, and
  backend state
- **Framework agnostic**: Works with Playwright, Puppeteer, or Selenium
- **Fast feedback**: Automated tests catch integration bugs immediately
- **Platform validation**: Tests can run with different platform configurations
- **Minimal production impact**: Headless mode is only used for testing,
  production code unchanged
- **Reusable**: Same headless infrastructure can be used for visual regression
  testing, performance testing
- **Developer-friendly**: Developers can run E2E tests locally with `--headless`
  flag
- **Test isolation**: Each test can start fresh application instance with clean
  state

### Negative

- **Added complexity**: New transport implementation and browser launcher code
  to maintain
- **Test infrastructure**: Requires Playwright/Puppeteer dependencies and
  knowledge
- **Different code path**: Headless mode uses different transport than
  production (risk of divergence)
- **Performance overhead**: Launching browser for each test adds time
- **Debugging challenge**: Headless tests are harder to debug than interactive
  UI
- **Maintenance burden**: Must maintain both Photino and headless browser paths
- **WebSocket complexity**: WebSocket transport is more complex than Photino's
  simple message API
- **Platform-specific limitations**: Some platform-specific features may not
  work in headless mode
- **Resource usage**: Running headless browser requires significant memory and
  CPU

### Mitigations

- **Share code**: Keep transport interface identical so Gateway and controllers
  work unchanged
- **Test parity**: Regularly verify headless mode behaves identically to Photino
  mode
- **Debugging tools**: Provide screenshot capture, video recording, and trace
  export for failed tests
- **Test helpers**: Build reusable test harnesses that simplify E2E test
  authoring
- **Parallel execution**: Run tests in parallel to reduce total execution time
- **Selective headless**: Only use headless mode for E2E tests, not
  unit/integration tests
- **Feature detection**: Mark tests as "headless-incompatible" if they require
  native features
- **CI optimization**: Cache browser binaries, reuse browser instances when
  possible
- **Logging bridge**: Ensure logs from both UI and backend are correlated in
  test output

## General Notes

### Why Headless Mode Instead of Alternatives?

**Option: Mock the UI**

- Pros: Fast, no browser needed
- Cons: Doesn't test real UI code, can't catch UI bugs
- Verdict: Good for controller/service tests, insufficient for E2E

**Option: Manual testing only**

- Pros: No code complexity
- Cons: Slow, error-prone, doesn't scale, blocks CI/CD
- Verdict: Not sustainable as application grows

**Option: Separate test build**

- Pros: Cleaner separation
- Cons: Requires maintaining parallel build, risks divergence
- Verdict: Too much overhead for marginal benefit

**Option: Headless mode flag**

- Pros: Minimal code changes, uses production code paths, enables automation
- Cons: Adds conditional logic, requires WebSocket transport
- Verdict: Best balance of automation capability vs. complexity

### Transport Layer Considerations

The key insight is that `ITransport` is already an abstraction—it doesn't matter
whether messages travel via Photino's `SendWebMessage` or WebSocket or HTTP. The
Gateway, controllers, and services don't know or care. This makes adding a
headless transport relatively straightforward.

The WebSocket transport in headless mode provides the same guarantees as Photino
transport:

- Bidirectional communication
- Message ordering
- Connection lifecycle management
- Error handling

### Platform-Specific Testing

The headless mode can simulate different platform configurations:

```csharp
var harness = await ZylanceTestHarness.CreateAsync(new TestConfig
{
    Platform = Platform.Desktop,
    OS = OperatingSystem.Windows,
    VaultProvider = new LocalVaultProvider()
});
```

This enables testing platform-specific code paths (file pickers, notifications,
etc.) without physical devices.

### Browser Choice: Chromium vs. Others

We recommend Chromium (via Playwright/Puppeteer) because:

- Fastest startup and execution
- Best tooling for automation (CDP protocol)
- Cross-platform support (Windows, macOS, Linux)
- Same engine as Photino uses on most platforms
- Excellent debugging tools (screenshots, traces, videos)

Firefox and WebKit can be added later if needed for cross-browser validation.

### Debugging Failed E2E Tests

When E2E tests fail, several debugging tools are available:

```csharp
var test = await ZylanceTestHarness.CreateAsync(new TestConfig
{
    CaptureScreenshots = true,
    RecordVideo = true,
    SaveTrace = true,
    SlowMotion = 1000 // Slow down actions for visibility
});
```

Playwright automatically saves these artifacts on failure, making it easy to
diagnose issues.

### Performance Optimization

Launching a browser for every test is expensive. Optimization strategies:

1. **Browser reuse**: Keep browser instance alive across tests, open new page
   per test
2. **Parallel execution**: Run tests in parallel with xUnit's test
   parallelization
3. **Test grouping**: Group related tests to share setup/teardown
4. **Selective E2E**: Only run E2E tests for critical paths, use integration
   tests for others
5. **Fast-fail**: Stop test run on first failure to save time during development

With these optimizations, E2E test suite can run in 1-3 minutes even with dozens
of tests.

### Real-World E2E Test Scenarios

Examples of valuable E2E tests:

- **Vault lifecycle**: Create vault → lock → unlock → close
- **Import flow**: Open vault → import QFX file → verify transactions appear
- **Budget management**: Create budget → add categories → verify totals
- **Account reconciliation**: Mark transactions → verify balance
- **Error handling**: Trigger backend error → verify UI shows error message
- **State synchronization**: Open multiple windows → verify state syncs
- **Platform features**: File picker on desktop vs. web vs. mobile

Each test validates multiple layers working together, catching integration bugs
that unit tests miss.

### Integration with Existing Tests

Headless E2E tests complement existing test layers:

```
Unit Tests (xUnit, Vitest)
  ↓ Test individual classes/functions
Integration Tests (xUnit)
  ↓ Test controllers + services
E2E Tests (xUnit + Playwright + Headless Mode)
  ↓ Test full stack
Manual Testing
  ↓ Exploratory testing, UX validation
```

Each layer catches different types of bugs. E2E tests fill the gap between
integration tests and manual testing.

### CI/CD Integration

In CI pipeline, E2E tests run after unit and integration tests:

```yaml
- name: Run Unit Tests
  run: dotnet test --filter Category=Unit

- name: Run Integration Tests
  run: dotnet test --filter Category=Integration

- name: Run E2E Tests
  run: dotnet test --filter Category=E2E
  env:
    ZYLANCE_HEADLESS: true
```

E2E tests act as final gate before deployment, ensuring all changes work
together.

### Future Enhancements

Once headless mode is established, additional capabilities become possible:

- **Visual regression testing**: Compare screenshots across commits
- **Performance profiling**: Measure UI load time, interaction latency
- **Accessibility testing**: Automated a11y audits via axe-core
- **Cross-platform matrix**: Run same tests on Windows, macOS, Linux
- **Mobile emulation**: Test mobile UI behavior in headless mode
- **Load testing**: Simulate multiple concurrent users

### Inspiration from Other Projects

This pattern is common in desktop applications that need automated testing:

- **VS Code**: Uses Electron with headless Chromium for E2E tests
- **Slack Desktop**: Automated E2E tests with headless browser
- **Discord**: E2E testing with Playwright
- **Figma Desktop**: Custom test harness with headless browser

The pattern: When you have a web-based UI in a desktop wrapper, headless browser
testing is the most practical E2E solution.

---

**For future blog post**: Could explore "Testing Desktop Apps with Web UIs":

- The challenge of E2E testing in Electron/Photino/Tauri apps
- Why traditional UI automation doesn't work for WebView apps
- Headless browser as test double for native window
- Balancing test coverage with test execution time
- Real bugs caught by E2E tests that unit tests missed
- CI/CD integration patterns for headless tests
- Debugging strategies for failed E2E tests
- Case study: Implementing headless E2E testing in Zylance
