# ADR-003: Vault Provider Abstraction Pattern

## Context

Zylance needs to support multiple storage backends for user financial data:
- **Local storage** (v1): SQLite database on the local filesystem
- **Remote sync** (future): Self-hosted or managed cloud storage with encryption
- **Third-party integrations** (future): Dropbox, Google Drive, iCloud, etc.

Each storage backend has different characteristics, APIs, and constraints. Without proper abstraction, adding new storage types would require changes throughout the codebase, violating the Open/Closed Principle.

We needed to decide:
1. How do we abstract storage implementations?
2. Should we use interfaces or abstract base classes?
3. What operations should the abstraction expose?
4. How do we handle vault lifecycle (create, open, close)?

## Decision

Use the **Provider Pattern** with the `IVaultProvider` interface to abstract all vault implementations.

The architecture consists of:

1. **`IVaultProvider` interface**: Defines vault lifecycle operations
   ```csharp
   public interface IVaultProvider
   {
       Task<IVault> OpenVault();
       Task<IVault> CreateVault();
   }
   ```

2. **`IVault` interface**: Defines vault data operations (implemented separately)

3. **Concrete implementations**: 
   - `LocalVaultProvider` (Zylance.Vault.Local)
   - `RemoteVaultProvider` (Zylance.Vault.Remote, future)
   - Additional providers for third-party services

4. **Dependency injection**: Vault providers are registered in the DI container and injected where needed

This means:
- Application code depends on `IVaultProvider`, not concrete implementations
- New vault types can be added without modifying existing code
- Vault implementations can be swapped via configuration
- Testing is simplified with mock vault providers
- Platform-specific vault logic is isolated in provider implementations

## Consequences

### Positive

- **Open for extension**: New vault types can be added without changing core code
- **Closed for modification**: Core application code doesn't change when adding vaults
- **Dependency inversion**: High-level code doesn't depend on low-level storage details
- **Testability**: Easy to mock vault providers for unit tests
- **Platform flexibility**: Desktop, web, and mobile can have platform-specific providers
- **Configuration-driven**: Vault selection can be a runtime configuration choice
- **Clean architecture**: Storage concerns are properly separated from business logic
- **Interface segregation**: Minimal interface with only essential operations

### Negative

- **Abstraction overhead**: Extra layer of indirection adds some complexity
- **Interface evolution**: Changes to `IVaultProvider` affect all implementations
- **Least common denominator**: Interface must work for all vault types, potentially limiting features
- **Async everywhere**: All vault operations must be async to support remote vaults
- **Discovery complexity**: Finding which vault provider is in use requires runtime inspection

### Mitigations

- Keep the interface minimal and stable
- Use extension methods for optional features that not all providers support
- Version the interface carefully and avoid breaking changes
- Document the provider contract clearly
- Consider using feature flags or capability detection for advanced features
- Provide diagnostic tools to inspect configured providers

## General Notes

The Provider Pattern is a tried-and-true approach for this kind of abstraction. It's similar to how .NET's logging (`ILoggerProvider`), configuration (`IConfigurationProvider`), and hosting (`IHostBuilder`) abstractions work. We're following established .NET patterns rather than inventing something new.

One key decision was keeping the interface minimal—just `OpenVault()` and `CreateVault()`. We deliberately didn't include vault enumeration, deletion, or other management operations in the provider interface. Those operations might not make sense for all providers (e.g., remote providers might not allow enumeration of all user vaults).

The async nature of the interface is important. Even though local vaults could be synchronous, making the interface async ensures it works for remote vaults where all operations require network I/O. This also future-proofs the API for scenarios we haven't considered yet.

The relationship between `IVaultProvider` and `IVault` is important. The provider is responsible for vault lifecycle (create/open), while the vault itself handles data operations. This separation of concerns makes each interface simpler and more focused.

Testing benefits are significant. During development, we can use a `MockVaultProvider` that returns an in-memory vault, making tests fast and isolated. In production, the same code can use `LocalVaultProvider` or `RemoteVaultProvider` without changes.

**For future blog post**: Could explore the Provider Pattern in depth, showing how it enables the Open/Closed Principle in practice. The progression from "hard-coded SQLite" to "abstracted provider pattern" to "multiple providers" is a great teaching example. Also worth discussing how abstraction boundaries should be chosen—too much abstraction is as bad as too little.
