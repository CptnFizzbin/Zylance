# ADR-005: Source Generators for Controller Auto-Registration

## Context

Zylance uses a controller-based architecture where controllers handle specific domains (File, Vault, Status, Echo). Each controller has methods decorated with `[RequestHandler]` or `[EventHandler]` attributes that need to be registered with the Gateway for message routing.

Traditional approaches to controller registration:
1. **Manual registration**: Explicitly register each controller method in startup code
2. **Runtime reflection**: Scan assemblies at startup to find and register controllers
3. **Source generators**: Generate registration code at compile time

Manual registration is tedious and error-prone—every new controller method requires boilerplate registration code. Runtime reflection works but has drawbacks:
- Performance overhead at startup
- Doesn't work in AOT scenarios (Native AOT, WASM)
- Makes code less discoverable
- Harder to debug and understand

We needed a solution that:
1. Eliminates manual registration boilerplate
2. Works with AOT and WASM
3. Provides compile-time safety
4. Maintains good IDE support

## Implementation

**Status**: Complete

## Decision

Use **C# source generators** to automatically discover controllers at compile time and generate registration code.

The implementation:
1. **`ZylanceSourceGenerator`**: Roslyn analyzer that scans for controllers
2. **Attribute-based discovery**: Controllers marked with `[Controller]` or methods with `[RequestHandler]`/`[EventHandler]`
3. **Generated registration code**: Creates `AddZylanceRouter()` extension method
4. **Compile-time**: All discovery happens during compilation, not at runtime
5. **Type-safe**: Generated code is strongly typed, no reflection

The source generator:
```csharp
[Generator]
public class ZylanceSourceGenerator : IIncrementalGenerator
{
    // Discovers controllers via syntax analysis
    // Generates registration code
}
```

Generated code example:
```csharp
public static class GeneratedControllerRegistration
{
    public static IServiceCollection AddZylanceRouter(this IServiceCollection services)
    {
        services.AddSingleton<FileController>();
        services.AddSingleton<VaultController>();
        // ... auto-generated for all controllers
        return services;
    }
}
```

## Consequences

### Positive

- **Zero boilerplate**: Developers just add attributes, registration is automatic
- **AOT compatible**: No runtime reflection means works with Native AOT and WASM
- **Compile-time safety**: Errors in controller setup are caught during compilation
- **IDE support**: Generated code is available to IDEs (IntelliSense, Go to Definition)
- **Performance**: No reflection overhead at startup
- **Discoverability**: Easy to see what's registered by looking at generated code
- **Maintainability**: Less code to maintain, no registration bookkeeping
- **Consistency**: All controllers are registered the same way

### Negative

- **Build complexity**: Adds compile-time code generation step
- **Debugging**: Source generators can be harder to debug than regular code
- **IDE caching**: Sometimes IDEs don't update generated code until rebuild
- **Learning curve**: Team needs to understand how source generators work
- **Generator bugs**: Bugs in the generator affect all controller registrations
- **Build time**: Source generators add to compilation time
- **Tooling dependency**: Requires Visual Studio 2022+ or recent Rider

### Mitigations

- Enable `EmitCompilerGeneratedFiles` for debugging (outputs to `obj/`)
- Write comprehensive tests for the source generator
- Document source generator behavior for team
- Keep generator logic simple and focused
- Use incremental generation for better performance
- Provide clear error messages when generator fails
- Add analyzer warnings for common mistakes (e.g., missing attributes)

## General Notes

This decision was influenced by a **Copilot suggestion** during early development. While reviewing controller registration code, Copilot suggested using source generators to eliminate the boilerplate. This was a valuable suggestion that we investigated and adopted.

Source generators are a C# 9+ feature that enables compile-time metaprogramming. They're similar to T4 templates or code generation scripts, but better integrated into the build process. The key advantage is that generated code is available to the IDE immediately after compilation.

The source generator uses incremental generation (`IIncrementalGenerator`), which means it only regenerates code when relevant source files change. This keeps build times reasonable even in large projects.

**Implementation insights:**
- The generator uses Roslyn syntax analysis to find controller classes
- It checks for `[Controller]` attribute or presence of handler attributes
- Generated code is added to the compilation as additional source files
- Diagnostics from the generator show up as build warnings/errors

The WASM compatibility was a critical factor. We plan to support a web version of Zylance that could run entirely in the browser via WebAssembly. WASM and Native AOT both restrict or prohibit runtime reflection, making source generators essential for scenarios that traditionally used reflection.

**Comparison with other frameworks:**
- ASP.NET Core MVC uses runtime reflection for controller discovery
- ASP.NET Core Minimal APIs use source generators in .NET 7+
- Blazor uses source generators for component parameter binding
- Entity Framework Core uses source generators for compiled queries

We're following the modern .NET pattern of "compile-time whenever possible."

One interesting aspect is that source generators enable new architectural patterns. For example, we could generate type-safe clients for our Protocol Buffers messages, or auto-implement repository patterns, or generate validation code. The controller registration generator is just the first application.

**For future blog post**: This could be a great blog post about practical source generator usage. Topics: when to use source generators vs. reflection, debugging techniques, performance comparisons, WASM/AOT considerations, and the "Copilot suggested it" origin story. Also worth discussing how AI coding assistants can suggest architectural patterns you weren't considering.
