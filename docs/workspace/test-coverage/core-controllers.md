# Core — Controllers

Four controllers (`Echo`, `File`, `Ledger`, `Accounts`) already have dedicated
test files under `tests/Zylance.Core.Tests/Router/Controllers/`. Three do not.
The goal of this task is to bring them to parity.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Core/Router/Controllers/ImportController.cs` | ~209 | No tests |
| `src/Zylance.Core/Router/Controllers/VaultController.cs` | ~72 | No tests |
| `src/Zylance.Core/Router/Controllers/StatusController.cs` | ~29 | No tests |

## Why it matters

Controllers are the contract surface exposed to the UI. They translate
incoming requests into vault/service calls, surface errors, and emit events.
Without tests, refactoring services tends to silently break controllers and
ship as a runtime UI failure.

## What to test

### `ImportController` (largest gap)
This is a multi-step import flow with significant branching. Cover at minimum:
- **Happy path** — a valid OFX file produces an `ImportResult` with the
  expected statements and accounts.
- **File-selection / file-missing** — the response when the supplied path
  does not exist or is unreadable.
- **Parse failures** — malformed OFX content surfaces a structured error,
  not a raw exception.
- **Cancellation** — confirm the controller honors a cancellation token mid
  import.
- **Background task events** — the controller is expected to publish
  `BackgroundWork*Evt` events; assert they fire with consistent `taskId`s
  for start/progress/finish.
- **Account creation** — when imported transactions reference unknown
  accounts, verify the controller's chosen behavior (create vs. fail vs.
  prompt).

### `VaultController`
- Open / unlock / close lifecycle.
- Behavior when a vault is already open (idempotent? error?).
- Error mapping — confirm `VaultException` paths are translated to the
  documented error response, not raw exceptions.
- Status reporting — that the controller updates whatever shared state the
  rest of the app reads.

### `StatusController`
- Returns the application's current status payload.
- Behavior when no vault is open vs. open vs. locked.
- That the response shape matches the contract record exactly (this is the
  primary thing UI tests will rely on).

## Implementation pointers

- **Mirror the existing controller tests.** The closest analogues are:
  - `LedgerControllerTests.cs` — for state-changing controllers with
    pagination/cursors (parallels `ImportController`'s streaming nature).
  - `AccountsControllerTests.cs` — for CRUD-shaped controllers (parallels
    `VaultController`).
  - `EchoControllerTests.cs` / `FileControllerTests.cs` — for very small
    controllers (parallels `StatusController`).
- **Use the test factories** in `tests/Zylance.Core.Tests/TestUtils/Factories/`
  to build vault contexts (`VaultContextTestFactory`,
  `VaultTestFactory`) — do not stand up a real DB.
- **Fixtures.** OFX import tests can lean on
  `tests/Zylance.Core.Tests/TestUtils/Fixtures/FixtureUtils.cs` and the
  existing OFX sample files used by `OfxParserTests`.
- **Event assertions.** Use `TestTransport` to capture sent events and assert
  on them; this is the same pattern other controller tests use to verify
  side-effects.
- **Cancellation tokens.** Pass `TestContext.Current.CancellationToken` to
  every awaited call — xUnit1051 will fail the build otherwise.
- **Records / `init`.** Construct request/response DTOs using object
  initializer syntax to match the codebase style.

## Out of scope

- E2E-style tests covering the entire UI ↔ backend round-trip — those belong
  in `Zylance.Desktop.Tests`.
- Importer parsing logic — that is covered separately in
  [`core-importers.md`](./core-importers.md).
