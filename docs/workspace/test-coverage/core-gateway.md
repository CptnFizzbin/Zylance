# Core — Gateway pipeline

The Gateway is the central message router between the UI and backend. It
receives transport payloads, dispatches them to controllers, and emits events
back. Today only `GatewayService` and `RequestHandlerUtils` have direct tests.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Core/Gateway/Handlers/ZyRequestHandler.cs` | ~26 | No tests |
| `src/Zylance.Core/Gateway/Handlers/ZyEventHandler.cs` | ~22 | No tests |
| `src/Zylance.Core/Gateway/Handlers/ExceptionHandler.cs` | ~29 | No tests |
| `src/Zylance.Core/Gateway/Utils/EventHandlerUtils.cs` | — | No tests (peer of `RequestHandlerUtils` which *is* tested) |
| `src/Zylance.Core/Gateway/Models/ZyEvent.cs` | — | No direct tests |

## Why it matters

Every request and event in the system flows through these handlers. A
regression here breaks all UI ↔ backend communication and is hard to detect
through controller-level tests alone, because handler logic short-circuits
before reaching them.

## What to test

### `ZyRequestHandler`
- Routes a request to the correct controller method based on the `Action`
  field.
- Returns a well-formed `ZyResponse` for successful invocations.
- Surfaces exceptions through `ExceptionHandler` rather than letting them
  escape.
- Handles unknown actions (no registered handler) with a meaningful error
  response, not a crash.

### `ZyEventHandler`
- Dispatches an event to all registered handlers.
- Confirms handlers can be invoked concurrently without deadlocks (if the
  implementation supports concurrency).
- Tolerates a handler throwing without preventing other handlers from
  running.

### `ExceptionHandler`
- Maps `VaultException` and `CursorException` to their expected response
  shapes / error codes.
- Wraps unexpected exceptions in a generic error response without leaking
  internal detail (assert the message is sanitized).
- Preserves the original exception for logging (verify via the injected
  logger or a captured `ILogger` mock).

### `EventHandlerUtils`
- Mirror the assertions in the existing `RequestHandlerUtils` tests.
- Verify reflection-based handler discovery picks up methods decorated with
  `[EventHandler]` and rejects methods without the attribute.
- Verify parameter-binding behavior for the event payload type.

## Implementation pointers

- **Mirror existing patterns.** `tests/Zylance.Core.Tests/Gateway/Services/GatewayServiceTests.cs`
  and `tests/Zylance.Core.Tests/Router/RouterServiceTests.cs` already exercise
  the broader pipeline. Reuse their setup style.
- **Test transport** is already provided: `tests/Zylance.Core.Tests/TestUtils/Mocks/TestTransport.cs`.
  Use it instead of mocking `ITransport` from scratch.
- **Test factories** in `tests/Zylance.Core.Tests/TestUtils/Factories/` build
  `ZyRequest`, `ZyResponse`, and vault contexts. Extend them rather than
  inventing new ones.
- **`InternalsVisibleTo`** is already configured for `Zylance.Core.Tests` —
  you can reach internal handler members directly.
- **Async + cancellation:** every handler exposes async APIs; pass
  `TestContext.Current.CancellationToken` to every awaited call.
- **Exception assertions:** prefer `Assert.IsType<T>` + property checks over
  `Assert.Throws` when the handler is supposed to *catch* the exception and
  return an error response.

## Out of scope

- Performance / throughput benchmarks.
- Cross-process transport tests (covered by `Zylance.Desktop.Tests` E2E).
