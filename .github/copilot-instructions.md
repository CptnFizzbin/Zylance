# Zylance Project - Copilot Instructions

## Project Overview

Zylance is an open-source finance and budgeting application built with a clean architecture approach. The application
features a desktop interface powered by Photino.NET with a React/TypeScript frontend, communicating via Protocol Buffers
over a custom transport layer.

## Architecture

### Core Components

- **Zylance.Core** - Core business logic with controllers, services, and Gateway for message routing
- **Zylance.Desktop** - Desktop application using Photino.NET for native windowing
- **Zylance.UI** - React + TypeScript + Vite frontend
- **Zylance.Contract** - Protocol Buffers message contracts for type-safe communication
- **Zylance.Vault.Local** - Local vault implementation using Entity Framework Core
- **Zylance.SourceGenerators** - Source generators for automatic controller registration

### Key Patterns

- **Dependency Injection**: Uses `Microsoft.Extensions.DependencyInjection` throughout
- **Gateway Pattern**: Central message router (`Gateway.cs`) handles request/response and events
- **Controller Pattern**: Controllers handle specific domains (File, Vault, Status, Echo)
- **Provider Pattern**: Platform-specific implementations via `ITransport`, `IFileProvider`, `IVaultProvider`

### Communication Flow

1. UI sends requests via transport layer (Protocol Buffers)
2. Gateway receives and routes to appropriate controller
3. Controller processes request and returns response
4. Gateway sends response back to UI

## Code Style Guidelines

### Null Checking

✅ **Prefer pattern matching over equality operators:**

```csharp
// Good
if (value is null) { }
if (value is not null) { }

// Avoid
if (value == null) { }
if (value != null) { }
```

**Why?** Pattern matching (`is null`/`is not null`) is more consistent with modern C# patterns, provides better type
narrowing, and is the preferred style in C# 9+.

### DTOs and POCOs

✅ **Prefer `record` types over `class` for data transfer objects and plain data structures:**

```csharp
// Good - immutable record
public record UserDto(string Id, string Name, string Email);

// Good - record with init-only properties
public record UserDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
}

// Avoid for DTOs/POCOs
public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}
```

**Why?** Records provide:

- Value-based equality by default
- Immutability (when using positional syntax or `init`)
- Concise syntax with positional parameters
- Better for data that shouldn't change after creation
- Clearer intent that this is data, not behavior

### Property Initialization

✅ **Use `init` accessors for DTOs and POCOs instead of `set`:**

```csharp
// Good - init-only properties
public record RequestDto
{
    public required string Action { get; init; }
    public string? DataJson { get; init; }
}

// Avoid - mutable properties
public class RequestDto
{
    public string Action { get; set; }
    public string? DataJson { get; set; }
}
```

**Why?** Init-only properties:

- Allow object initialization syntax while preventing mutation after construction
- Make data immutability explicit and enforced by the compiler
- Reduce bugs from unintended state changes
- Work well with `required` keyword for mandatory properties
- Better express that DTOs/POCOs are immutable data contracts

### Additional Guidelines

- Use `required` keyword for mandatory properties on records/classes
- Leverage source generators for repetitive code (see `Zylance.SourceGenerators`)
- Follow async/await patterns for I/O operations
- Use nullable reference types (`string?`) to express nullability explicitly
- Controllers should be stateless and rely on injected services

## Technology Stack

- **.NET 10.0** - Target framework
- **Photino.NET** - Native desktop windowing
- **React + TypeScript** - Frontend UI
- **Vite** - Frontend build tool
- **Protocol Buffers** - Serialization format
- **Entity Framework Core** - Database ORM (Local vault)
- **Roslyn Source Generators** - Code generation
