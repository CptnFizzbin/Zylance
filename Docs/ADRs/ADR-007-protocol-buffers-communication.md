# ADR-007: Protocol Buffers for Type-Safe Client-Server Communication

## Context

Zylance uses a client-server architecture where the UI (React/TypeScript)
communicates with the backend (C#/.NET) across a boundary. This boundary exists
in all supported platforms:

- **Desktop**: UI runs in a WebView, backend in native process
- **Web** (future): UI in browser, backend in WASM
- **Mobile** (future): UI in WebView, backend in native app

This cross-boundary communication requires:

1. A serialization format for messages
2. Type definitions understood by both TypeScript and C#
3. Support for request/response and event patterns
4. Forward/backward compatibility for versioning

Options considered:

- **JSON**: Universal, human-readable, but no type safety across languages
- **gRPC**: Strongly typed, but heavyweight and HTTP/2 specific
- **Protocol Buffers**: Schema-first, type-safe, cross-platform
- **Custom format**: Full control, but significant work

We needed a solution that provides type safety, works across languages, is
maintainable, and supports our architectural goals (including future WASM
support).

## Implementation

**Status**: Complete

## Decision

Use **Protocol Buffers (protobuf)** as the serialization format and contract
definition language, with auto-generated TypeScript types from the C# contract
definitions.

The implementation:

1. **Contract project** (`Zylance.Contract`): Defines all messages in `.proto`
   files
2. **Code generation**:
    - C# types generated via `protoc` and `Google.Protobuf`
    - TypeScript types generated via `protoc` and `protobuf-ts`
3. **Shared types**: Both languages use generated code from the same `.proto`
   definitions
4. **Transport agnostic**: Protobuf messages can be sent over any transport (
   WebSocket, HTTP, IPC)
5. **Schema-first**: Contract is explicitly defined and version-controlled

Message structure:

```protobuf
message EchoReq {
  string message = 1;
}

message EchoRes {
  string echo = 1;
}
```

Both C# and TypeScript get strongly-typed classes/interfaces from this
definition.

## Risks

- Running Zylance natively on Android and iOS may not be possible, will need to
  investigate further. As a last resort, Zylance can run as a PWA at the very
  least.

## Consequences

### Positive

- **Type safety across languages**: TypeScript and C# types are guaranteed to
  match
- **Compile-time checking**: Breaking changes to contracts fail at compile time
- **Cross-platform**: Protocol Buffers work on all platforms (desktop, web,
  mobile, WASM)
- **Schema as documentation**: `.proto` files are readable contracts
- **Tooling**: Excellent IDE support for protobuf
- **Ecosystem**: Many tools and libraries for protobuf
- **Generated code**: No manual serialization/deserialization code

### Negative

- **Build complexity**: Code generation step required
- **Learning curve**: Team must understand protobuf syntax and semantics
- **Schema rigidity**: Contract changes require updates on both sides
- **Additional dependency**: Need protobuf runtime libraries

### Mitigations

- Add logging of serialized messages during development
- Use protobuf linting to catch common mistakes
- Keep `.proto` files well-documented with comments
- Provide message pretty-printing utilities for debugging

## General Notes

Protocol Buffers are Google's language-neutral, platform-neutral, extensible
mechanism for serializing structured data. They're used extensively at Google
and in gRPC. The key advantage for Zylance is type safety across the
TypeScript/C# boundary.

**Why protobuf over JSON:**

- JSON requires manual synchronization of types between languages
- Easy to have subtle mismatches (field names, types, nullable vs. non-nullable)
- No compile-time checking that client and server agree on format

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

**Schema evolution guidelines** (note: since this is an IPC contract and we're
not using binary strings, we don't need to follow these guidelines strictly.
Good to still keep in mind though):

- Never change field numbers (breaks binary compatibility)
- Use `reserved` for removed fields to prevent reuse
- Add new fields with default values
- Use `optional` for fields that might not be present
- Increment version numbers for major changes

**Real-world benefits observed:**

1. **Refactoring safety**: Renaming a field in `.proto` causes compile errors in
   both C# and TypeScript
2. **API discoverability**: Generated types make API usage clear in IDE
3. **Testing**: Can generate test fixtures from protobuf schemas
4. **Documentation**: Schema serves as machine-readable API documentation

**Alternative considered: Custom serialization**
We briefly considered a custom serialization format tailored to our specific
needs. While this would give maximum control, the maintenance burden wasn't
worth the marginal benefits. Protobuf is battle-tested and well-supported.

---

**For future blog post**: Could write about "Type Safety Across Language
Boundaries" covering:

- The problem: maintaining API contracts between languages
- Why JSON isn't enough (type drift, manual synchronization)
- How Protocol Buffers solve this (schema-first, code generation)
- Real examples of bugs prevented by protobuf type checking
- Trade-offs and when JSON might still be better
- Integration with TypeScript and C# ecosystems
