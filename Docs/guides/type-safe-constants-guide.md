# Type-Safe Action and Event Names - Usage Guide

This guide explains how to use the automatically generated type-safe constants for actions and events in the Zylance project.

## Problem Solved

Previously, action and event names were string literals scattered throughout the codebase:

```typescript
// ❌ Old way - prone to typos, no autocomplete
client.createRequestEndpoint<"Vault:OpenVault", ...>("Vault:OpenVault")
client.createEventListener<"Vault:VaultOpened", ...>("Vault:VaultOpened")
```

This approach had several issues:
- **No type checking** - Typos like `"Vault:OpnVault"` only fail at runtime
- **No autocomplete** - Developers had to remember exact names
- **Hard to refactor** - Changing a name required finding all string occurrences
- **No single source of truth** - Names duplicated across proto files, C#, and TypeScript

## Solution

Action and event names are now automatically extracted from Protocol Buffer definitions and generated as type-safe constants during the build process.

```typescript
// ✅ New way - type-safe, autocomplete, refactor-friendly
import { ZylanceActions, ZylanceEvents } from "../Generated/ZylanceConstants"

client.createRequestEndpoint(ZylanceActions.Vault_OpenVault)
client.createEventListener(ZylanceEvents.Vault_VaultOpened)
```

## How It Works

### 1. Define in Proto Files (Single Source of Truth)

Actions and events are defined once in `.proto` files:

```protobuf
// zylance/api/Vault.proto
message VaultOpenReq {
  option (action) = "Vault:OpenVault";
}

message VaultOpenedEvt {
  option (eventName) = "Vault:VaultOpened";
}
```

### 2. Automatic Generation During Build

When you run `dotnet build` or `yarn build:contract`, the build process:

1. Compiles all `.proto` files to TypeScript and C#
2. Runs `Scripts/generate-constants.ts` to extract action/event names
3. Generates:
   - `Zylance.Contract/Generated/ZylanceConstants.cs`
   - `Zylance.Contract/Generated/ZylanceConstants.ts` (reference copy)
   - `Zylance.UI/Generated/ZylanceConstants.ts` (used by UI)

### 3. Use in Your Code

**TypeScript:**

```typescript
import { ZylanceActions, ZylanceEvents, type ZylanceAction, type ZylanceEvent } from "../Generated/ZylanceConstants"

// Request endpoints
const openVault = client.createRequestEndpoint(
  ZylanceActions.Vault_OpenVault
)

// Event listeners
const unsubscribe = client.createEventListener(
  ZylanceEvents.Vault_VaultOpened,
  (data) => console.log("Vault opened!", data)
)

// Type-safe variables
const action: ZylanceAction = ZylanceActions.Echo_EchoMessage // ✓ Valid
const badAction: ZylanceAction = "Invalid:Action" // ✗ Compile error!
```

**C#:**

```csharp
using Zylance.Contract;

// C# controllers typically don't need constants (they extract from proto messages)
// But they're available for logging, debugging, or manual routing:

Console.WriteLine($"Processing action: {ZylanceConstants.Actions.Vault_OpenVault}");
Console.WriteLine($"Emitting event: {ZylanceConstants.Events.Vault_VaultOpened}");
```

## Naming Convention

Proto names use `Namespace:Action` format, which become constants with underscores:

| Proto File | Proto Declaration | TypeScript Constant | C# Constant |
|------------|-------------------|---------------------|-------------|
| Vault.proto | `option (action) = "Vault:OpenVault"` | `ZylanceActions.Vault_OpenVault` | `ZylanceConstants.Actions.Vault_OpenVault` |
| Background.proto | `option (eventName) = "Background:WorkStart"` | `ZylanceEvents.Background_WorkStart` | `ZylanceConstants.Events.Background_WorkStart` |

## Adding New Actions/Events

To add a new action or event:

1. **Add to proto file:**
   ```protobuf
   message MyNewReq {
     option (action) = "MyDomain:MyAction";
   }
   ```

2. **Rebuild:**
   ```bash
   dotnet build Zylance.Contract
   ```

3. **Use the constant:**
   ```typescript
   import { ZylanceActions } from "../Generated/ZylanceConstants"
   
   client.createRequestEndpoint(ZylanceActions.MyDomain_MyAction)
   ```

That's it! No manual constant maintenance needed.

## Benefits

### 1. Type Safety
```typescript
// ✓ Valid - TypeScript knows this action exists
const action: ZylanceAction = ZylanceActions.Vault_OpenVault

// ✗ Compile error - TypeScript catches typo
const bad: ZylanceAction = "Vault:OpenValt"
```

### 2. Autocomplete & Discovery
Your IDE will suggest all available actions/events as you type:
- Type `ZylanceActions.` → See all request actions
- Type `ZylanceEvents.` → See all event names

### 3. Refactoring Support
Renaming `"Vault:OpenVault"` to `"Vault:Open"` in the proto file automatically:
- Updates the generated constant name
- Causes compile errors in all usage sites
- Allows safe, complete refactoring

### 4. Self-Documenting Code
```typescript
// Before: What actions are available? Who knows!
client.createRequestEndpoint("SomeAction")

// After: Clear, discoverable, documented
client.createRequestEndpoint(ZylanceActions.Vault_OpenVault)
```

## Troubleshooting

### Constants Not Found
If TypeScript can't find the constants:

1. **Rebuild contracts:**
   ```bash
   cd Zylance.Contract
   dotnet build
   ```

2. **Check Generated folder exists:**
   ```bash
   ls -la Zylance.UI/Generated/ZylanceConstants.ts
   ```

3. **Verify import path:**
   ```typescript
   import { ZylanceActions, ZylanceEvents } from "../Generated/ZylanceConstants"
   ```

### Constants Out of Date
If you added a new action but the constant doesn't exist:

```bash
# Rebuild to regenerate constants
dotnet build Zylance.Contract
```

The constants are automatically regenerated on every build, so they should always be up-to-date.

## Advanced: Manual Regeneration

To manually regenerate constants without a full build:

```bash
cd Zylance.Contract
npx tsx Scripts/generate-constants.ts
```

This is useful for debugging or if you only want to update the constants without compiling proto files.

## See Also

- **[Generated/README.md](../../Zylance.Contract/Generated/README.md)** - Technical details about generation
- **[ADR-007-protocol-buffers-communication.md](../ADRs/ADR-007-protocol-buffers-communication.md)** - Protocol Buffers architecture decision
- **[Zylance.Contract/Scripts/generate-constants.ts](../../Zylance.Contract/Scripts/generate-constants.ts)** - Generation script source code
