# Zylance.Contract

Protocol Buffer message definitions for type-safe communication between the UI and Core.

## Overview

Zylance.Contract uses Protocol Buffers (protobuf) to define the message types and API contracts shared between the UI layer and the Core business logic. This ensures type-safe, efficient serialization and deserialization across the application boundary.

## What's Inside

### Message Definitions (.proto files)
- **Request/Response messages** - For synchronous operations
- **Event messages** - For asynchronous notifications
- **Data models** - Common data structures used throughout the application

### Generated Code
The project automatically generates code for both:
- **C# classes** - Used by `Zylance.Core` and platform hosts
- **TypeScript types** - Used by `Zylance.UI`

## Why Protocol Buffers?

1. **Type Safety** - Compile-time checking across language boundaries
2. **Versioning** - Built-in support for backward compatibility
3. **Cross-Platform** - Works seamlessly between C# and TypeScript
4. **Schema First** - API contract is explicitly defined and shared

## Building

```bash
# Generate C# and TypeScript code from .proto files
npm run build
```

The build process:
1. Compiles `.proto` files to C# using `protoc`
2. Compiles `.proto` files to TypeScript using `protobuf-ts`
3. Outputs generated files for consumption by other projects

## Usage

### In C# (Core/Desktop)
```csharp
using Zylance.Contract;

var request = new SomeRequest 
{ 
    Field1 = "value",
    Field2 = 42
};

// Serialize and send via transport
```

### In TypeScript (UI)
```typescript
import { SomeRequest } from '@zylance/contract';

const request: SomeRequest = {
    field1: 'value',
    field2: 42
};

// Serialize and send via transport
```

## Adding New Messages

1. Create or modify `.proto` files in the appropriate directory
2. Run `npm run build` to regenerate code
3. Both C# and TypeScript will automatically get the new types

## Dependencies

- **protoc** - Protocol Buffer compiler
- **protobuf-ts** - TypeScript code generator
- **Google.Protobuf** - C# runtime library

