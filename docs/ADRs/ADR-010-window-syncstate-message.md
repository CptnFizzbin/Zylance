# ADR-010: WindowSyncStateReq Message for UI-Core State Synchronization

## Context

When the Zylance application window loads, the UI needs to synchronize its state with the backend Core to ensure it accurately reflects the current application state. This is particularly critical for:

1. **Vault state**: Is a vault open? Is it locked or unlocked? Which vault is active?
2. **Background tasks**: Are there any ongoing operations (imports, exports, syncs)?
3. **Application settings**: User preferences, theme, locale
4. **Window state**: Maximized, minimized, position (desktop only)

Currently, the UI performs state synchronization through individual queries on component mount:

```typescript
// ZylanceProvider queries vault status on mount
const { data: vaultStatus } = useQuery({
  queryKey: ["vault", "status"],
  queryFn: async () => {
    const status = await zylanceApi.vault.getStatus()
    return status.vaultRef ?? null
  },
  staleTime: Number.POSITIVE_INFINITY,
})
```

This approach has several limitations:

1. **Scattered initialization**: Different components query different state independently
2. **Race conditions**: Multiple simultaneous queries on startup create unnecessary load
3. **Incomplete state**: Easy to miss critical state that should be synced on load
4. **No coordination**: No single point to ensure all state is synced before rendering
5. **Redundant queries**: Multiple components might query the same state independently

As the application grows, we need a coordinated, centralized mechanism to sync all relevant state in a single request when the window loads.

Options considered:

- **Status quo (individual queries)**: Simple but doesn't scale, creates race conditions
- **Multiple parallel queries**: Better but still uncoordinated, hard to know when "ready"
- **Single comprehensive sync message**: Clean, coordinated, explicit about what's needed
- **State streaming on connect**: Complex, may send unnecessary state updates

We needed a solution that provides a clean initialization point, coordinates state synchronization, and clearly documents what state the UI expects from Core on load.

## Implementation

**Status**: Planned

## Decision

Introduce a **`WindowSyncStateReq` / `WindowSyncStateRes`** message pair (with action `Window:SyncState`) that the UI calls once on window load to synchronize all relevant application state from Core in a single coordinated request.

The implementation:

1. **Protocol Buffer Definition**: Define the request/response messages in a new `Window.proto` file:

```protobuf
syntax = "proto3";
package zylance.contract;

import "zylance/extensions/Zylance.proto";
import "zylance/models/Vault.proto";

option csharp_namespace = "Zylance.Contract.Api.Window";

message WindowSyncStateReq {
  option (action) = "Window:SyncState";
}

message WindowSyncStateRes {
  option (action) = "Window:SyncState";
  
  // Vault state
  optional VaultRef current_vault = 1;
  
  // Background tasks (future)
  // repeated BackgroundTaskStatus background_tasks = 2;
  
  // Application settings (future)
  // ApplicationSettings settings = 3;
  
  // Window state (desktop only, future)
  // WindowState window_state = 4;
}

// See "Window:Heartbeat Event" section below for WindowHeartbeatEvt definition
```

2. **Controller Implementation**: Create a `WindowController` in Core:

```csharp
[Controller]
public class WindowController(VaultService vaultService)
{
    [RequestHandler]
    public void SyncState(ZyRequest<WindowSyncStateReq> req, ZyResponse<WindowSyncStateRes> res)
    {
        var currentVault = vaultService.GetActiveVaultRef();
        
        res.SetData(new WindowSyncStateRes 
        { 
            CurrentVault = currentVault
        });
    }
}
```

3. **UI Integration**: Call `Window:SyncState` once in `ZylanceProvider` on mount:

```typescript
const { data: syncedState } = useQuery({
  queryKey: ["window", "syncState"],
  queryFn: async () => await zylanceApi.window.syncState(),
  staleTime: Number.POSITIVE_INFINITY,
})

useEffect(() => {
  if (!syncedState) return
  setCurrentVault(syncedState.currentVault ?? null)
  // Set other state as it's added to WindowSyncStateRes
}, [syncedState])
```

4. **Gradual Migration**: Initially implement with just vault state, then gradually add more state fields as needed. The old individual queries can remain during transition and be removed once `Window:SyncState` fully replaces them.

### Window:Heartbeat Event

In addition to the sync state request, introduce a **`WindowHeartbeatEvt`** event that the UI sends periodically to Core to indicate the window is alive and responsive:

```protobuf
message WindowHeartbeatEvt {
  option (eventName) = "Window:Heartbeat";
  
  // UUIDv7 identifying this window instance
  // Generated on window creation and persists for window lifetime
  string window_id = 1;
}
```

**Purpose**: Allows Core to detect:
- **Hung UI**: If heartbeats stop arriving, the UI may be frozen or unresponsive
- **Restarted UI**: A new `window_id` indicates the window was closed and reopened
- **Multiple windows**: Each window instance has a unique ID (future multi-window support)

**Implementation**:
- UI sends heartbeat every 30 seconds (configurable)
- Core tracks last heartbeat timestamp per `window_id`
- Core can emit warnings or take action if heartbeat is overdue
- UUIDv7 provides time-ordered IDs for debugging and correlation

**Use cases**:
- Debugging: Correlate Core logs with specific window instances
- Recovery: Core can clean up resources for dead windows
- Monitoring: Track UI responsiveness in production
- Multi-window: Distinguish between multiple simultaneous windows

Key principles:

- **Single call on load**: UI makes one `Window:SyncState` request when initializing
- **Idempotent**: Safe to call multiple times (e.g., on reconnect)
- **Optional fields**: Use `optional` in protobuf so fields can be added without breaking changes
- **Namespace separation**: `Window:*` namespace for window lifecycle messages
- **Versioning friendly**: Can add new fields to response without breaking old clients

## Consequences

### Positive

- **Coordinated initialization**: Single point to sync all state on window load
- **Reduced race conditions**: One request instead of many parallel queries
- **Clear contract**: Explicitly documents what state UI needs from Core
- **Performance**: Single round-trip instead of multiple independent queries
- **Discoverability**: Easy to find all state that's synced on load
- **Testing**: Can mock entire initial state in one response
- **Debugging**: Can log complete initial state synchronization
- **Future-proof**: Easy to add new state fields without changing pattern
- **Type safety**: Protocol Buffers ensure UI and Core agree on state shape
- **Ready signal**: UI knows when initial state is fully synced

### Negative

- **Additional message**: One more message type to maintain
- **All-or-nothing**: If one state piece is slow, entire response is delayed
- **Growing response**: As app grows, response may become large
- **Duplication during transition**: Temporary duplication with existing queries
- **Not real-time**: Only syncs on load, not on state changes (that's what events are for)

### Mitigations

- Keep response focused on truly load-critical state
- Use events for ongoing state changes after initial load
- Consider splitting into multiple sync messages if response becomes too large
- Document which fields are required vs. optional
- Implement timeout handling for slow state queries
- Use optional fields to allow incremental additions
- Consider caching strategies if state assembly is expensive

## General Notes

The `Window:SyncState` message represents a common pattern in client-server architectures: the "initial handshake" where client and server agree on starting state. Without this pattern, initialization becomes ad-hoc and fragile.

**Why a dedicated Window namespace:**

In Protocol Buffers communication, we organize messages by domain (Vault, File, Ledger). Window lifecycle events (load, close, focus, blur) are their own domain, distinct from business logic. The `Window:` namespace makes this clear and provides a natural home for other window-related messages in the future.

**Window:Heartbeat for liveness detection:**

The `WindowHeartbeatEvt` complements the sync state request by providing ongoing liveness monitoring. Unlike the sync request which is called once on load, heartbeats are sent continuously to prove the UI is responsive.

**Why UUIDv7 for window_id:**
- **Time-ordered**: UUIDv7 embeds timestamp, making IDs naturally sortable by creation time
- **Unique**: Globally unique across all windows and sessions
- **Debugging**: Timestamp embedded in ID helps correlate events temporally
- **Standards-compliant**: UUIDv7 is the latest UUID standard (RFC 9562)

The heartbeat interval (30 seconds by default) balances:
- **Network overhead**: Frequent heartbeats waste bandwidth
- **Detection latency**: Infrequent heartbeats delay hung UI detection
- **Battery life**: On mobile, minimize background activity

Core behavior on missing heartbeats:
- **Grace period**: Allow 2-3 missed heartbeats before considering UI hung
- **No automatic action**: Core should log warnings but not take destructive action
- **Cleanup on reconnect**: When UI reconnects with new `window_id`, clean up old resources

This pattern is common in distributed systems:
- **HTTP keep-alive**: Similar concept for connection liveness
- **gRPC health checks**: Standard health checking protocol
- **WebSocket ping/pong**: Protocol-level liveness detection
- **Database connection pools**: Validate connections before use

**What belongs in WindowSyncStateRes:**

Not all state should be in the sync response. Good candidates are:

- State that's critical for rendering the initial UI
- State that rarely changes or changes through explicit actions
- State that's expensive to query multiple times
- State that multiple components need

Poor candidates are:

- State that's component-specific and only needed deep in the tree
- State that changes frequently (use events instead)
- Large datasets (use lazy loading/pagination instead)
- State that's only needed after user interaction

**Evolution strategy:**

Start small with just vault state, the most critical piece. As we identify other state that's consistently queried on load, add it to `WindowSyncStateRes`. This gradual approach prevents over-engineering while establishing the pattern.

**Relationship to existing queries:**

`Window:SyncState` doesn't replace all queries. It replaces initialization queries that happen on every window load. Component-specific queries that happen based on user navigation are still appropriate (e.g., loading a specific ledger's transactions).

**Desktop vs. Web vs. Mobile:**

The response can include platform-specific state (like window position on desktop) using optional fields. The UI ignores fields that aren't relevant to its platform. This keeps a single message type while allowing platform-specific concerns.

**Error handling:**

If `Window:SyncState` fails, the UI should show a clear error and provide a retry mechanism. This failure is different from individual query failures because it blocks the entire app initialization. Consider implementing automatic retry with exponential backoff.

**Real-world analogies:**

- **HTTP**: Similar to an `/api/init` or `/api/bootstrap` endpoint
- **GraphQL**: Similar to a query that fetches multiple resources in one request
- **Mobile apps**: Similar to splash screen loading all initial data
- **Games**: Similar to loading screen that fetches player state, inventory, world state

**Testing benefits:**

With `Window:SyncState`, tests can mock the entire initial state in one place rather than mocking multiple individual query responses. This makes test setup clearer and reduces test brittleness:

```typescript
it("renders locked UI when vault is locked", () => {
  mockWindowSyncState({ 
    currentVault: { locked: true, path: "/path/to/vault" } 
  })
  render(<App />)
  expect(screen.getByText("Unlock Vault")).toBeInTheDocument()
})
```

**Performance considerations:**

One concern might be that gathering all state for the response is expensive. However:

1. Most state is already cached in services (like `VaultService`)
2. The alternative (many parallel queries) does the same work but in parallel
3. We can optimize the controller to gather state efficiently
4. We can add caching if assembly becomes a bottleneck

In practice, a single well-optimized query is often faster than many small parallel queries due to reduced overhead.

**Migration path:**

1. **Phase 1**: Add `Window.proto`, `WindowController`, and basic vault state
2. **Phase 2**: Update `ZylanceProvider` to call `Window:SyncState`
3. **Phase 3**: Gradually add more state fields (background tasks, settings)
4. **Phase 4**: Remove redundant individual initialization queries
5. **Phase 5**: Document that `Window:SyncState` is the canonical initialization point

This phased approach allows for gradual adoption without a big-bang migration.

---

**For future blog post**: Could explore the "Initial Handshake" pattern in distributed systems:

- Why client-server apps need coordinated initialization
- Common anti-patterns (scattered queries, race conditions)
- The sync message pattern and its benefits
- Protocol Buffers for versioned sync contracts
- Testing strategies for initial state
- Performance considerations (one query vs. many)
- How this pattern appears in various frameworks (REST, GraphQL, WebSocket)
- Case study: Implementing Window:SyncState in Zylance
