# Test Coverage Improvement Tasks

This workspace folder catalogs concrete, scoped tasks for raising test coverage
across the Zylance codebase. Each document focuses on a single area, lists the
files that lack tests, explains *why* they matter, and gives implementors
specific pointers on what to assert.

## How to use this folder

- Pick a single document and treat it as a self-contained work item (or split
  it into multiple PRs if it is large).
- Each task lists **Target files**, **Why it matters**, **What to test**, and
  **Implementation pointers** (existing test patterns to mirror, gotchas, etc).
- Prefer extending existing test projects over adding new ones. The layout
  under `tests/` mirrors `src/` — keep that mirror intact.

## Documents

| Area | Document | Priority |
| --- | --- | --- |
| Core — Gateway pipeline | [core-gateway.md](./core-gateway.md) | High |
| Core — Controllers | [core-controllers.md](./core-controllers.md) | High |
| Core — Vault services | [core-vault-services.md](./core-vault-services.md) | High |
| Core — System & logging | [core-system-logging.md](./core-system-logging.md) | High |
| Core — Importers (OFX) | [core-importers.md](./core-importers.md) | Medium |
| Vault.Local | [vault-local.md](./vault-local.md) | Medium |
| Vault.Remote | [vault-remote.md](./vault-remote.md) | Low |
| Desktop | [desktop.md](./desktop.md) | Medium |
| Frontend (Zylance.UI) | [frontend-ui.md](./frontend-ui.md) | High (no tests today) |

## Recommended order of attack

1. **`ZylanceCore` DI smoke test** (see `core-system-logging.md`) — single
   test, broad protection against registration regressions.
2. **Controller parity** (`core-controllers.md`) — `ImportController`,
   `VaultController`, `StatusController` should match the coverage of
   `EchoController`, `FileController`, `LedgerController`,
   `AccountsController`.
3. **`BackgroundTaskService`** (`core-system-logging.md`) — concurrency code
   with no tests.
4. **`LocalMetadataManager`** (`vault-local.md`) — bring vault-manager
   coverage to parity with ledger/account managers.
5. **Gateway handlers** (`core-gateway.md`) — small files with high
   blast-radius if they regress.
6. **Frontend test foundation** (`frontend-ui.md`) — add Vitest, then start
   with `WebSocketTransport`, `ZylanceClient`, and `ImportContext`.
7. **`WebsocketTransport` (.NET side)** (`desktop.md`) — pair with the TS
   counterpart so both sides of the protocol are covered.
8. **`OfxImportParser` error paths** (`core-importers.md`) — extend the
   existing thin smoke-test file.

## Conventions to follow

These are enforced by the codebase (see the repository copilot instructions
for the full list); call them out in any new test you write:

- **xUnit v3** — use `[Theory]` + `[InlineData]` for parameterized cases. Test
  names follow `Method_Scenario_ExpectedResult`.
- **Cancellation tokens** — pass `TestContext.Current.CancellationToken` to
  every `async` call that accepts one (xUnit1051 will fail the build
  otherwise).
- **Records over classes** for DTOs; `init` accessors over `set`.
- **Pattern-matched null checks** (`is null` / `is not null`).
- **Full, descriptive variable names** — do not abbreviate.
- **CSharpier** — run `dotnet csharpier .` before committing.
- **`InternalsVisibleTo`** — internal classes are made testable via the
  project's `Properties/AssemblyInfo.cs`. Add the test assembly there if you
  need to reach an internal type.

## Running the tests

```bash
# All .NET tests
dotnet test

# A single test class (xUnit v3 syntax — note the v2 --filter does not work)
dotnet test --filter-class "*BackgroundTaskServiceTests"

# Frontend (once Vitest is added — see frontend-ui.md)
cd src/Zylance.UI && pnpm test
```

## What this folder is *not*

- It is **not** a coverage report. There is no instrumented coverage run
  driving these gaps; they were identified by cross-referencing source files
  against test files by name. Some files marked "untested" may be exercised
  indirectly via integration or E2E tests — when in doubt, add a unit test
  anyway; integration coverage rarely substitutes for fast targeted tests.
- It is **not** prescriptive about test count. Aim for behavior coverage, not
  line count.
