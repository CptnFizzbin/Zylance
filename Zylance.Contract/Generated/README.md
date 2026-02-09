# Generated Type-Safe Constants

This directory contains auto-generated type-safe constants for all action and event names defined in the Protocol Buffer contracts.

## Files

- **`ZylanceConstants.ts`** - TypeScript constants and type definitions
- **`ZylanceConstants.cs`** - C# constants

## Generation

These files are automatically generated during the build process by the `Scripts/Lib/generate-constants.ts` script, which:

1. Scans all `.proto` files in the `zylance/api/` directory
2. Extracts action names from `option (action) = "..."` declarations
3. Extracts event names from `option (eventName) = "..."` declarations
4. Generates type-safe constants for both TypeScript and C#

## Usage

### TypeScript

```typescript
import { ZylanceActions, ZylanceEvents, type ZylanceAction, type ZylanceEvent } from "../Generated/ZylanceConstants"

// Use constants instead of string literals
const client = new ZylanceClient()

// Actions
client.createRequestEndpoint(ZylanceActions.Vault_OpenVault)
client.createRequestEndpoint(ZylanceActions.File_SelectFile)

// Events
client.createEventListener(ZylanceEvents.Vault_VaultOpened)
client.createEventEmitter(ZylanceEvents.Desktop_Exit)

// Type-safe action/event name variables
const action: ZylanceAction = ZylanceActions.Echo_EchoMessage // ✓ Valid
const event: ZylanceEvent = ZylanceEvents.Background_WorkStart // ✓ Valid
```

**Benefits:**
- **Autocomplete**: IDE suggests all available actions/events
- **Type safety**: Typos caught at compile time
- **Refactoring**: Renaming an action/event updates all usages
- **Single source of truth**: All names come from `.proto` files

### C#

In C#, action and event names are typically extracted from protobuf messages at runtime using `ProtoActionUtils.GetAction<T>()` and `ProtoActionUtils.GetEventName<T>()`. However, the generated constants are available for scenarios where you need direct string access:

```csharp
using Zylance.Contract;

// Reference constants when needed
var action = ZylanceConstants.Actions.Vault_OpenVault;
var eventName = ZylanceConstants.Events.Vault_VaultOpened;

// Useful for logging, debugging, or manual routing
Console.WriteLine($"Handling action: {ZylanceConstants.Actions.File_SelectFile}");
```

**Note:** C# controllers using `[RequestHandler]` and `[EventHandler]` attributes automatically extract action/event names from protobuf messages, so explicit constants are rarely needed.

## Naming Convention

Action and event names use the format `Namespace:Action` in proto files (e.g., `"Vault:OpenVault"`), which become constants with underscores replacing colons:

| Proto Declaration | TypeScript Constant | C# Constant |
|-------------------|---------------------|-------------|
| `option (action) = "Vault:OpenVault"` | `ZylanceActions.Vault_OpenVault` | `ZylanceConstants.Actions.Vault_OpenVault` |
| `option (eventName) = "Vault:VaultOpened"` | `ZylanceEvents.Vault_VaultOpened` | `ZylanceConstants.Events.Vault_VaultOpened` |

## Regeneration

To manually regenerate these files, build the contract project:

```bash
dotnet build Zylance.Contract.csproj
```

## Do Not Edit

**⚠️ WARNING:** These files are auto-generated. Any manual changes will be overwritten during the next build.

To add or modify action/event names, update the corresponding `.proto` files in `zylance/api/`.
