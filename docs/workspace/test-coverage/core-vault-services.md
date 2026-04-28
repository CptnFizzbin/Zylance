# Core — Vault services & exceptions

`VaultContext` has direct tests, but the orchestration services that sit
above it do not. Those services own vault lifecycle and account-level
operations and are reached by every controller that touches vault state.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Core/Vault/Services/VaultService.cs` | ~73 | No direct tests; exercised indirectly via `VaultContextTests` |
| `src/Zylance.Core/Vault/Services/AccountService.cs` | ~38 | No direct tests |
| `src/Zylance.Core/Vault/Exceptions/VaultException.cs` | — | Construction / message not asserted |
| `src/Zylance.Core/Vault/Exceptions/CursorException.cs` | — | Construction / message not asserted |

## Why it matters

These services are the seam between controllers and the pluggable vault
providers (`Zylance.Vault.Local`, `Zylance.Vault.Remote`). A bug here
surfaces as either data corruption or "vault won't open" — both are
high-severity user-visible failures.

## What to test

### `VaultService`
- **Open / unlock / close transitions** — drive each transition and assert
  the resulting `VaultContext` state.
- **Provider selection** — given multiple registered `IVaultProvider`s,
  verify the correct one is chosen (e.g. by scheme or configuration).
- **Re-entrancy** — opening an already-open vault, closing an already-closed
  vault. Assert the documented behavior (idempotent vs. throw) is consistent.
- **Error translation** — when a provider throws, confirm a `VaultException`
  surfaces with a useful message and inner exception preserved.

### `AccountService`
- CRUD against a fake `IAccountManager` (an in-memory test double, not EF).
- That ordering / sorting matches the behavior expected by the UI ledger.
- Validation of inputs (empty name, duplicate id, etc) — assert the chosen
  policy.

### Exception classes
- Each constructor produces the expected `Message` content.
- Custom properties (e.g. `NonZylanceDatabaseException.Reason`) round-trip.
- Verify exceptions are `[Serializable]` only if they need to be — otherwise
  just assert the primary-constructor pattern works as documented.

## Implementation pointers

- **Existing analogue:** `tests/Zylance.Core.Tests/Vault/Context/VaultContextTests.cs`
  is the closest reference — start there for fixture style.
- **Test doubles:** prefer hand-rolled in-memory implementations of
  `IVaultProvider`, `IAccountManager`, `IMetadataManager`, etc. The
  Vault.Remote tests already do this for storage interfaces — see
  `tests/Zylance.Vault.Remote.Tests/Search/BucketedSearch/Lib/Memory*.cs`
  for the convention.
- **Do not pull in `Zylance.Vault.Local`** for these tests. That couples
  Core tests to EF Core and SQLite.
- **Cancellation:** every async API on these services accepts a
  `CancellationToken`; thread `TestContext.Current.CancellationToken`
  through and add at least one test that cancels mid-call.
- **Exception assertions:** use `Assert.Throws<VaultException>` and assert
  on `.Message` / inner exception, not on string formatting that could
  change.

## Out of scope

- Provider-specific behaviour (covered by `vault-local.md` and
  `vault-remote.md`).
- DI registration (covered by `core-system-logging.md`'s `ZylanceCore` DI
  smoke test).
