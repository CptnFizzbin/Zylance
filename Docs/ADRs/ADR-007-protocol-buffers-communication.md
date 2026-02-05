# ADR-007: Protocol Buffers for Type-Safe Client-Server Communication

## Context

Zylance uses a client-server architecture where the UI (React/TypeScript) communicates with the backend (C#/.NET) across a boundary. This boundary exists in all supported platforms:
- **Desktop**: UI runs in a WebView, backend in native process
- **Web** (future): UI in browser, backend on server or via WASM
- **Mobile** (future): UI in WebView, backend in native app

This cross-boundary communication requires:
1. A serialization format for messages
2. Type definitions understood by both TypeScript and C#
3. Support for request/response and event patterns
4. Forward/backward compatibility for versioning

Options considered:
- **JSON**: Universal, human-readable, but no type safety across languages
- **MessagePack**: Binary and efficient, but no schema
- **gRPC**: Strongly typed, but heavyweight and HTTP/2 specific
- **Protocol Buffers**: Schema-first, type-safe, cross-platform
- **Custom format**: Full control, but significant work

We needed a solution that provides type safety, works across languages, is maintainable, and supports our architectural goals (including future WASM support).

## Decision

Use **Protocol Buffers (protobuf)** as the serialization format and contract definition language, with auto-generated TypeScript types from the C# contract definitions.

The implementation:
1. **Contract project** (`Zylance.Contract`): Defines all messages in `.proto` files
2. **Code generation**: 
   - C# types generated via `protoc` and `Google.Protobuf`
   - TypeScript types generated via `protobuf-ts`
3. **Shared types**: Both languages use generated code from the same `.proto` definitions
4. **Transport agnostic**: Protobuf messages can be sent over any transport (WebSocket, HTTP, IPC)
5. **Schema-first**: Contract is explicitly defined and version-controlled

Message structure:
```protobuf
message EchoRequest {
  string message = 1;
}

message EchoResponse {
  string echo = 1;
}
```

Both C# and TypeScript get strongly-typed classes/interfaces from this definition.

## Consequences

### Positive

- **Type safety across languages**: TypeScript and C# types are guaranteed to match
- **Compile-time checking**: Breaking changes to contracts fail at compile time
- **Cross-platform**: Protocol Buffers work on all platforms (desktop, web, mobile, WASM)
- **Efficient serialization**: Binary format is smaller and faster than JSON
- **Backward compatibility**: Protobuf has built-in versioning support
- **Schema as documentation**: `.proto` files are readable contracts
- **Tooling**: Excellent IDE support for protobuf
- **Ecosystem**: Many tools and libraries for protobuf
- **Generated code**: No manual serialization/deserialization code

### Negative

- **Binary format**: Not human-readable (harder to debug than JSON)
- **Build complexity**: Code generation step required
- **Learning curve**: Team must understand protobuf syntax and semantics
- **Schema rigidity**: Contract changes require updates on both sides
- **Debugging difficulty**: Can't inspect messages in network debugger without special tools
- **Schema evolution**: Must carefully manage breaking vs. non-breaking changes
- **Additional dependency**: Need protobuf runtime libraries

### Mitigations

- Use protobuf debugging tools (like `protoc --decode`) for message inspection
- Add logging of serialized messages during development (with opt-in toggle)
- Document schema evolution guidelines (how to add/deprecate fields)
- Use protobuf linting to catch common mistakes
- Keep `.proto` files well-documented with comments
- Provide message pretty-printing utilities for debugging
- Use JSON as fallback serialization for debugging scenarios

## General Notes

Protocol Buffers are Google's language-neutral, platform-neutral, extensible mechanism for serializing structured data. They're used extensively at Google and in gRPC. The key advantage for Zylance is type safety across the TypeScript/C# boundary.

**Why protobuf over JSON:**
- JSON requires manual synchronization of types between languages
- Easy to have subtle mismatches (field names, types, nullable vs. non-nullable)
- No compile-time checking that client and server agree on format
- No built-in versioning story

**Why protobuf over gRPC:**
- gRPC is great but opinionated about transport (HTTP/2)
- We need flexibility for desktop IPC and potential WASM scenarios
- gRPC is heavier than we need for simple request/response
- We can add gRPC later if needed (it uses protobuf underneath)

**The contract project structure:**
```
Zylance.Contract/
├── zylance/
│   ├── api/         # Request/response messages
│   │   ├── Echo.proto
│   │   ├── Vault.proto
│   │   └── File.proto
│   └── models/      # Shared data models
│       ├── Account.proto
│       └── Ledger.proto
└── build/           # Generated code output
```

**Schema evolution guidelines:**
- Never change field numbers (breaks binary compatibility)
- Use `reserved` for removed fields to prevent reuse
- Add new fields with default values
- Use `optional` for fields that might not be present
- Increment version numbers for major changes

**Real-world benefits observed:**
1. **Refactoring safety**: Renaming a field in `.proto` causes compile errors in both C# and TypeScript
2. **API discoverability**: Generated types make API usage clear in IDE
3. **Testing**: Can generate test fixtures from protobuf schemas
4. **Documentation**: Schema serves as machine-readable API documentation

**WASM considerations:**
Protocol Buffers work great with WebAssembly. The C# protobuf library works in WASM, and we can pass serialized bytes between JS and WASM efficiently. This was a key factor in choosing protobuf over alternatives.

**Alternative considered: Custom serialization**
We briefly considered a custom serialization format tailored to our specific needs. While this would give maximum control, the maintenance burden wasn't worth the marginal benefits. Protobuf is battle-tested and well-supported.

**Debugging story:**
The binary format can be challenging during development. To address this:
- Added message logging (opt-in via environment variable)
- Created `DebugTransport` that logs all messages as JSON
- Use browser DevTools for UI-side message inspection
- Provide protobuf text format conversion tools

**Performance:**
For our use case (desktop app with local communication), protobuf performance is overkill—we'd be fine with JSON. But the type safety and cross-platform benefits outweigh any downsides. Plus, the efficiency will help when we add remote sync features.

**For future blog post**: Could write about "Type Safety Across Language Boundaries" covering:
- The problem: maintaining API contracts between languages
- Why JSON isn't enough (type drift, manual synchronization)
- How Protocol Buffers solve this (schema-first, code generation)
- Real examples of bugs prevented by protobuf type checking
- Trade-offs and when JSON might still be better
- Integration with TypeScript and C# ecosystems
