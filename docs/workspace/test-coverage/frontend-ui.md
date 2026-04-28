# Frontend (Zylance.UI) — bootstrap a test foundation

The React/TypeScript frontend has **zero tests** today: there are no
`*.test.ts(x)` or `*.spec.ts(x)` files anywhere under
`src/Zylance.UI/`, and no test runner is configured.

This document is therefore split into two phases: **(1)** add a test
runner, and **(2)** target the highest-value modules.

## Phase 1 — Add a test runner

### Recommendation: Vitest

- Vite is already the build tool (`src/Zylance.UI/vite.config.ts`).
- Vitest reuses the same config, supports JSX/TSX out of the box, and
  pairs naturally with React Testing Library for component tests.
- Avoid Jest — it would require a parallel toolchain.

### What to add
- `vitest` and `@testing-library/react` as dev dependencies.
- A `test` script in `src/Zylance.UI/package.json`.
- A `vitest.config.ts` (or merged `vite.config.ts`) configured with
  `environment: 'jsdom'` and the project's existing path aliases.
- A Biome rule update if needed to recognize `*.test.tsx` files.
- One sample passing test to verify CI integration before scaling out.
- CI wiring in `.github/workflows/` so frontend tests run on PRs.

### Conventions to enforce
- Co-locate tests with the file under test:
  `Components/Common/HexagonSpinner.test.tsx`.
- Follow the existing component export style: named `export const` with
  typed `FC` (see the project copilot instructions).
- Use `import type` for type-only imports to satisfy the
  `verbatimModuleSyntax` setting.

## Phase 2 — Target the highest-value modules

Once the foundation is in place, prioritize in this order.

### 1. Transport & API client (highest leverage)
Files:
- `src/Zylance.UI/Src/Apis/Zylance/Transports/WebSocketTransport.ts`
- `src/Zylance.UI/Src/Apis/Zylance/ZylanceClient.ts`
- `src/Zylance.UI/Src/Apis/Zylance/ZylanceApi.ts`

What to test:
- Frame encode / decode round-trips for each message type.
- Request → response correlation (the client must match a response back
  to the request that originated it, even out of order).
- Timeout / cancellation: a request rejects when no response arrives.
- Reconnect: pending requests are either re-sent or rejected with a
  defined error — assert which.
- Pair these tests with the .NET-side `WebsocketTransport` tests in
  [`desktop.md`](./desktop.md) so both halves are covered.

### 2. Endpoint modules (`Apis/Zylance/Endpoints/*`)
- `StatusApi`, `AccountApi`, `FileApi`, `DesktopApi`, `ImportApi`,
  `EchoApi`, `VaultApi`, `LedgerApi`, `BackgroundApi`.
- One test per endpoint: it builds the right `ZyRequest` and parses the
  response into the expected typed result.
- Mock `ZylanceClient` rather than the transport for these.

### 3. `ImportContext` state machine
File: `src/Zylance.UI/Src/Components/Import/ImportContext.tsx` plus the
seven `DialogContent/*` components.

This is the most logic-dense piece of the UI. Test the state machine
directly (no DOM):
- Initial state is `SelectFile`.
- Each transition (file selected → reading → accounts → importing →
  finished | cancelled | error) fires only when the documented
  precondition is met.
- Cancellation from each interruptible state lands in `Cancelled`.
- Errors from any state land in `Error` with the message preserved.
- Background-task events update progress without changing state.

### 4. Hooks
- `Hooks/UseZylance.ts` — returns the same client across renders.
- `Components/Background/useBackgroundTasks.ts` — accumulates progress
  events, removes finished tasks.
- `Components/Runtime/Hooks/UseRuntime.ts`.
- `Components/Import/UseImportForm.ts` — TanStack Form integration.

Use `renderHook` from `@testing-library/react`. For hooks that depend on
the API client, wrap with the `ZylanceContext` provider and inject a
fake client.

### 5. Form fields
Files under `src/Zylance.UI/Src/Integrations/tanstack-form/Fields/`:
- `TextField.tsx`, `SelectField.tsx`, `CheckboxField.tsx`,
  `FilePickerField.tsx`.

For each field:
- Renders with required ARIA attributes.
- Calls the form `onChange` with the right value type.
- Surfaces validation errors from the form context.

### 6. Routes (auth gating)
Files under `src/Zylance.UI/Src/Routes/`:
- `locked/*` — when no vault is open, the user sees the unlock / select
  screen.
- `vault/*` — when a vault is open, the locked routes redirect away.

These can be tested with TanStack Router's `createMemoryRouter` and
React Testing Library — no need to mount the full app.

## Implementation pointers

- **Mock surface:** every test should mock at the
  `ITransport` boundary (the lowest layer), or the
  `ZylanceClient` boundary (one layer up), depending on what is under
  test. Do not stub individual endpoint functions in component tests —
  it makes the tests fragile.
- **Existing E2E tests** in `tests/Zylance.Desktop.Tests/E2E/` use the
  Photino harness end-to-end. Frontend unit tests should be entirely
  separate and fast (sub-second per file).
- **`routeTree.gen.ts`** is generated — never hand-edit it, never assert
  against its contents.
- **MUI components** can be rendered with a thin `ThemeProvider` from
  `Integrations/mui/Theme.ts` to avoid theme-context warnings.
- **Biome** runs as the linter — keep imports sorted and type-only
  imports explicit so the lint step stays green.

## Out of scope

- Visual regression / snapshot tests of large component trees — costly
  to maintain and out of scope for this initial coverage push.
- Replacing the existing Photino E2E tests.
