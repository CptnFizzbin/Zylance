# Vault.Local

`Zylance.Vault.Local` already has tests for `LedgerCursor`,
`LocalLedgerManager`, `LocalAccountManager`, and the marker table.
Metadata, scope, and context-factory wiring are missing.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Vault.Local/Managers/LocalMetadataManager.cs` | ~40 | No tests (peer of Account / Ledger managers, which **are** tested) |
| `src/Zylance.Vault.Local/LocalVaultScope.cs` | — | No tests |
| `src/Zylance.Vault.Local/Context/LocalVaultContextFactory.cs` | — | No tests |
| `src/Zylance.Vault.Local/Entities/AccountEntity.cs` | — | Constraints not asserted |
| `src/Zylance.Vault.Local/Entities/ZylanceMetadataEntity.cs` | — | Constraints not asserted |
| `src/Zylance.Vault.Local/Entities/LedgerEntryEntity.cs` | — | Constraints not asserted |

## Why it matters

`LocalMetadataManager` reads/writes the `_zylance_` marker table that
identifies a SQLite file as a Zylance vault. Bugs here cause "vault won't
open" or worse, "vault opens but is silently corrupt" failures.

`LocalVaultScope` and `LocalVaultContextFactory` are how the vault is
plugged into DI; a regression breaks every vault operation.

## What to test

### `LocalMetadataManager`
- **`GetAsync` for unknown key** returns `null`.
- **`SetAsync` then `GetAsync`** round-trips the value.
- **`SetAsync` updates an existing key** rather than inserting a duplicate
  (verify by row count).
- **Concurrency** — two `SetAsync` calls in flight do not corrupt the
  table (test against the same DbContext factory if the manager is meant
  to be safe; otherwise document that it isn't).
- **Cancellation** — passing an already-canceled token throws
  `OperationCanceledException` before the row is written.
- **Max length** — `Key` and `Value` have `[MaxLength(255)]`. Verify
  exceeding the limit produces a meaningful error (or is documented as
  truncated).

### `LocalVaultScope`
- Resolves `IAccountManager`, `ILedgerManager`, `IMetadataManager` to the
  Local* implementations.
- Disposing the scope disposes the underlying `DbContext`.

### `LocalVaultContextFactory`
- Creates a context bound to the supplied SQLite path.
- Applies migrations on first open (or rejects an un-migrated DB —
  whichever is the documented contract).
- Throws `NonZylanceDatabaseException` for SQLite files that lack the
  `_zylance_` marker table — this is the existing contract worth pinning
  down with a test.
- Cleans up file handles on failure (assert via `File.Delete` not
  throwing `IOException`).

### Entity constraint sanity (optional, low value)
- A single integration test that inserts a row violating
  `[MaxLength]` / `[Key]` and asserts EF surfaces a
  `DbUpdateException`. This guards against accidental annotation removal.

## Implementation pointers

- **Mirror existing manager tests.** `LocalAccountManagerTests.cs` and
  `LocalLedgerManagerTests.cs` are the templates — copy their setup.
- **`TestDbContextFactory`** in
  `tests/Zylance.Vault.Local.Tests/TestUtils/Factories/TestDbContextFactory.cs`
  already builds an in-memory / temporary SQLite context — reuse it.
- **Cancellation tokens.** Pass `TestContext.Current.CancellationToken` to
  every awaited DB call to avoid xUnit1051.
- **Data annotations style.** When asserting entity constraints, do not
  introduce Fluent API — the codebase prefers data annotations.
- **`init` accessors.** Entity property tests should construct entities
  via object initializers; do not change `init` to `set` to make a test
  easier.

## Out of scope

- Cross-vault data migration tests.
- Performance benchmarks.
