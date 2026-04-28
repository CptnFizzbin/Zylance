# Desktop (Photino host)

`Zylance.Desktop.Tests` contains E2E tests that exercise the desktop host
end-to-end, but several units inside the project have no targeted tests
of their own. The pieces below carry real logic (not just bootstrap) and
will benefit most from focused unit tests.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Desktop/Transports/WebsocketTransport.cs` | ~87 | No unit tests; exercised indirectly via E2E |
| `src/Zylance.Desktop/Services/WebServerService.cs` | ~92 | No tests |
| `src/Zylance.Desktop/Utils/WebUtils.cs` | ~54 | No tests; pure utility |
| `src/Zylance.Desktop/Configuration/ZyLoggerConfiguration.cs` | — | No tests |
| `src/Zylance.Desktop/Configuration/ZyConfiguration.cs` | — | No tests |
| `src/Zylance.Desktop/Providers/DesktopFileProvider.cs` | — | Only via E2E |
| `src/Zylance.Desktop/Providers/DesktopVaultProvider.cs` | — | Only via E2E |
| `src/Zylance.Desktop/Providers/LocalFileProvider.cs` | — | Only via E2E |
| `src/Zylance.Desktop/Program.cs` | — | Only via E2E |

## Why it matters

`WebsocketTransport` is the .NET half of the protocol that the React UI
uses; it is the .NET counterpart to `Zylance.UI/.../WebSocketTransport.ts`.
If either side regresses, every UI feature breaks. E2E tests are slow and
catch failures late.

`WebServerService` controls when the embedded server can serve requests —
race conditions here cause flaky startup.

## What to test

### `WebsocketTransport`
- **Frame round-trip** — given a byte payload, the transport sends it on
  the underlying socket exactly once, in the expected framing.
- **Receive dispatch** — incoming frames are dispatched to the registered
  handler with the original payload bytes intact.
- **Reconnect / disconnect** — disposing the transport closes the socket
  cleanly; double-dispose is safe.
- **Backpressure / send while disconnected** — assert the documented
  behavior (queue? throw? drop?).
- **Cancellation** — async send / receive honors the cancellation token.

Use a fake `WebSocket` (or a loopback pair via
`System.Net.WebSockets.WebSocket.CreateFromStream`) instead of standing up
a real HTTP listener — keeps the test fast and deterministic.

### `WebServerService`
- **Start / Stop lifecycle** — repeated start is idempotent or throws per
  contract; stop is safe to call before start.
- **Port selection** — confirm the chosen port is honored and a port-in-use
  failure surfaces a useful error.
- **Static asset serving** (if applicable) — a known-good asset path
  returns 200 with the expected MIME type.

### `WebUtils`
- Each utility method gets a `[Theory]` with happy-path and edge-case
  rows. Pure functions, no setup needed.

### `ZyLoggerConfiguration` / `ZyConfiguration`
- Default values are what the documentation claims.
- Configuration is applied to the logger / app pipeline (assert via the
  built `ServiceProvider` or via a captured Serilog `LoggerConfiguration`).
- Environment-variable / config-file overrides work.

### Providers
For each `*Provider`, write unit tests against the public interface
(`IFileProvider`, `IVaultProvider`) using the local filesystem in a temp
directory:
- File / vault round-trip.
- Non-existent path handling.
- Path traversal protection (e.g. that `../` inputs are rejected).

## Implementation pointers

- **Existing harness:** `tests/Zylance.Desktop.Tests/TestUtils/ZylanceTestHarness.cs`
  is for E2E. Do *not* extend it for unit tests; new unit tests should
  stand alone.
- **Headless file provider:** there is already a
  `tests/Zylance.Desktop.Tests/TestUtils/HeadlessFileProvider.cs` — model
  new test doubles after it.
- **Temp directories:** use `Path.Combine(Path.GetTempPath(),
  Guid.NewGuid().ToString())` and clean up in `Dispose` (xUnit v3
  `IAsyncLifetime`).
- **Async + CT:** every public API on these classes is async; pass
  `TestContext.Current.CancellationToken` everywhere xUnit1051 demands it.
- **Photino:** do not instantiate `PhotinoWindow` from unit tests — the
  hosting layer is what the existing E2E tests are for. Limit unit tests
  to types that have no native dependency.

## Out of scope

- The Photino window itself / native widget behavior — covered by E2E.
- Installer behavior (`installers/Zylance.Installer.Windows/`).
