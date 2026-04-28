# Vault.Remote

The bucketed search engine is well covered by
`BucketedSearchEngineTests` plus the `Memory*` in-memory test doubles.
The only remaining gap is the public abstraction.

## Target files

| File | Status |
| --- | --- |
| `src/Zylance.Vault.Remote/Search/IZkSearchEngine.cs` | Interface — no contract tests |

## Why it matters

`IZkSearchEngine` is the public seam that future remote-search
implementations must conform to. Today only `BucketedSearchEngine`
implements it, so the implementation tests effectively *are* the contract
tests. The moment a second implementation lands (or the existing one is
swapped), there is nothing to validate behavioral parity.

## What to test

Author a **base contract test class** that any `IZkSearchEngine`
implementation can be plugged into:

- Insert / search round-trip for single-keyword queries.
- Multi-keyword AND / OR semantics (whichever the contract defines).
- Direction handling — `SearchDirection.Forward` vs.
  `SearchDirection.Backward` produce mirrored ordering.
- Empty result set is returned, not null.
- Behavior on duplicate inserts of the same item.
- Behavior on cancellation mid-search (token propagates and throws
  `OperationCanceledException`).

Then, derive a concrete test class that runs the suite against
`BucketedSearchEngine` (using the existing `Memory*` test doubles).

## Implementation pointers

- **Pattern:** xUnit supports abstract base test classes — define
  `IZkSearchEngineContractTests<TEngine>` with `[Theory]` / `[Fact]`
  methods, then derive `BucketedSearchEngineContractTests :
  IZkSearchEngineContractTests<BucketedSearchEngine>`.
- **Reuse existing doubles** in
  `tests/Zylance.Vault.Remote.Tests/Search/BucketedSearch/Lib/`. Do not
  reimplement them.
- **Don't duplicate** the existing `BucketedSearchEngineTests` —
  internal-implementation behaviors stay there; only contract-level
  behaviors move into the new base class.
- **Cancellation tokens.** As elsewhere, pass
  `TestContext.Current.CancellationToken` to every awaited call.

## Out of scope

- A real (network-backed) remote vault implementation — none exists yet.
- ADR-002 (zero-knowledge) cryptographic guarantees, which belong in a
  separate security-focused test suite when an implementation lands.
