# Core — System services, logging, platform utilities, DI bootstrap

A handful of small but high-leverage utilities sit outside the Gateway /
Vault / Importers buckets. They are individually small, and a single PR can
reasonably cover this whole document.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Core/System/Services/BackgroundTaskService.cs` | ~99 | No tests (concurrency-sensitive) |
| `src/Zylance.Core/Logging/ZyLogger.cs` | ~48 | No tests |
| `src/Zylance.Core/Platform/FileRefUtils.cs` | ~17 | No tests; pure utility |
| `src/Zylance.Core/ZylanceCore.cs` | — | DI bootstrapper has no tests |

## Why it matters

- `BackgroundTaskService` emits the events that drive every progress
  indicator in the UI. If event ordering breaks, the import dialog state
  machine misbehaves silently.
- `ZyLogger` is the single point of context-enrichment for logs. A
  regression makes diagnostics in production effectively impossible.
- `FileRefUtils` is a pure utility — the cheapest possible test target.
- `ZylanceCore.cs` registers the DI graph. A single resolution test catches
  almost every "missing service" regression that today only fails at
  runtime.

## What to test

### `BackgroundTaskService`
- **`NotifyWorkStart` / `NotifyWorkProgress` / `NotifyWorkFinish`** each
  send exactly one event and the event payload matches the inputs. Capture
  events via `TestTransport`.
- **Progress clamping:** `NotifyWorkProgress` clamps `progress` to
  `[0.0, 1.0]`. Add `[InlineData(-1f, 0f)]`, `[InlineData(2f, 1f)]`,
  `[InlineData(0.5f, 0.5f)]`.
- **`WithProgress<T>` happy path:** wraps work, emits start → finish, and
  returns the work's result.
- **`WithProgress<T>` error path:** when the inner work throws, a `Finish`
  event is still emitted (with a "Failed: ..." description), and the
  exception is rethrown. Assert both: the event AND the throw.
- **`taskId` consistency:** within a single `WithProgress` call, every
  emitted event uses the same `TaskId`.

### `ZyLogger`
- `ForContext<T>()` returns a logger with the `SourceContext` enriched to
  `typeof(T).FullName` (or whatever convention the implementation uses).
- Loggers are reusable / safe to cache as `static readonly`.

### `FileRefUtils`
- Each public method, with at least one happy-path and one edge-case
  `[InlineData]` row.

### `ZylanceCore` DI smoke test
A single test that:
1. Builds a `ServiceProvider` using the same registrations the production
   bootstrap uses.
2. Resolves every public controller, service, and gateway.
3. Asserts no `InvalidOperationException` ("Unable to resolve service for
   type ...") is thrown.

This is the cheapest test in the codebase relative to the bugs it catches.

## Implementation pointers

- **Test transport:** capture events from `BackgroundTaskService` via
  `tests/Zylance.Core.Tests/TestUtils/Mocks/TestTransport.cs`. Don't mock
  `GatewayService`.
- **Logger assertions:** if a Serilog `ILogger` mock is needed, prefer the
  built-in `Serilog.Sinks.TestCorrelator` package over Moq — see Serilog
  docs. If that is not yet a dependency, simply assert observable behavior
  (events emitted) instead of log calls.
- **DI smoke test:** put it in a new file
  `tests/Zylance.Core.Tests/ZylanceCoreTests.cs`. Mirror the registration
  helper used by `Zylance.Desktop`'s `Program.cs` so the test stays in sync.
- **Concurrency:** `BackgroundTaskService` has no observable concurrency
  primitives, but `WithProgress` is async — pass
  `TestContext.Current.CancellationToken` through.

## Out of scope

- Logging sink configuration (covered by
  `Zylance.Desktop/Configuration/ZyLoggerConfiguration.cs`, see
  [`desktop.md`](./desktop.md)).
- End-to-end progress event delivery to the UI (covered by E2E tests in
  `Zylance.Desktop.Tests`).
