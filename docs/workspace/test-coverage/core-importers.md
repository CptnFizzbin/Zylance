# Core — Importers (OFX)

The OFX V1 raw parser and V1 parser have solid coverage already
(`OfxRawParserTests`, `OfxParserTests`). The public-facing
`OfxImportParser` and the V1 → cross-version model mapping are thinly
covered — only a 54-line smoke-test file exists.

## Target files

| File | LOC | Status |
| --- | --- | --- |
| `src/Zylance.Core/Importers/Ofx/OfxImportParser.cs` | ~71 | Only smoke-tested in `OfxImportParserTests.cs` |
| `src/Zylance.Core/Importers/Ofx/Extensions/StreamReaderExtensions.cs` | — | No tests |
| `src/Zylance.Core/Importers/Ofx/V1/Models/OfxV1Statement.cs` | — | Mapping logic untested |
| `src/Zylance.Core/Importers/Ofx/V1/Models/OfxV1Account.cs` | — | Mapping logic untested |
| `src/Zylance.Core/Importers/Ofx/V1/Models/OfxV1Balance.cs` | — | Mapping logic untested |
| `src/Zylance.Core/Importers/Ofx/V1/Models/OfxV1Transaction.cs` | — | Mapping logic untested |
| `src/Zylance.Core/Importers/Ofx/V1/Models/OfxV1TransactionList.cs` | — | Mapping logic untested |
| `src/Zylance.Core/Importers/Ofx/V1/Raw/OfxRawHeader.cs` | — | Header parsing not directly covered |
| `src/Zylance.Core/Importers/Ofx/V1/Raw/OfxTagNames.cs` | — | Constants — only worth testing if behavior depends on them |

## Why it matters

`OfxImportParser` is the entry point for every imported financial file —
it is the single thing a user is most likely to feed weird data into. Robust
error reporting here directly affects perceived product quality.

## What to test

### `OfxImportParser`
- **Format detection** — V1 headers, V2 (XML) headers, missing header,
  unrecognized header. Confirm the parser routes to the right inner parser
  or returns a structured "unsupported format" error.
- **`ParseResult` / `ImportResult` shapes** — confirm successful parses
  populate `Statements`, `Accounts`, errors/warnings collections, etc, in
  the documented way.
- **Error path: malformed body** — an OFX file with a valid header but a
  truncated/bad body produces a structured error, not an exception.
- **Error path: empty stream** — handled gracefully.
- **Encoding** — UTF-8 vs. Windows-1252 (OFX V1 frequently uses 1252);
  assert non-ASCII transaction memos round-trip correctly.

### V1 → cross-version model mapping
For each `OfxV1*` model:
- Mapping from raw `OfxRawElement` to the strongly-typed record produces the
  expected field values, including:
  - Date parsing edge cases (covered for `OfxTimeStamp`; ensure
    `OfxV1Transaction.DatePosted` flows through).
  - Amount parsing for negative values and decimal separators.
  - Optional fields (`MEMO`, `CHECKNUM`) — present and missing.
- Mapping rejects (or surfaces a warning for) required missing fields.

### `StreamReaderExtensions`
- Each extension method gets at least one happy-path and one edge-case
  test (empty stream, partial line, line with only whitespace).

### `OfxRawHeader`
- Parses each documented header form.
- Rejects malformed headers with a clear exception or `TryParse` returning
  false.

## Implementation pointers

- **Fixture files:** `tests/Zylance.Core.Tests/Importers/Ofx/V1/Parser/OfxParserTests.cs`
  already loads OFX samples — find them via the existing `FixtureUtils`. Add
  new fixtures alongside them, do not duplicate the loader.
- **Parameterized tests:** prefer `[Theory]` with `[InlineData]` for
  date/amount parsing variations. The existing `OfxTimeStampTests.cs` is the
  cleanest example to mirror.
- **Don't re-test the raw parser.** Mapping tests should construct the
  `OfxRawElement` graph by hand (or via a tiny helper) so failures pinpoint
  mapping bugs rather than parser bugs.
- **Records:** OFX models are records — use object initializer / positional
  syntax in test setup; do not introduce `set` accessors anywhere.
- **Encodings:** when adding fixture files in non-UTF-8 encoding, commit
  them as raw bytes (not text) and load via `File.ReadAllBytes`.

## Out of scope

- A V2 (XML-based OFX) parser — does not exist yet.
- The `ImportController` workflow that consumes these parse results — see
  [`core-controllers.md`](./core-controllers.md).
