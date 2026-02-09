# Zylance Project - Copilot Instructions

## Project Overview

Zylance is an open-source finance and budgeting application built with a clean
architecture approach. The application
features a desktop interface powered by Photino.NET with a React/TypeScript
frontend, communicating via Protocol Buffers
over a custom transport layer.

# Notes to Agents

You are encouraged to update this file with any additional instructions or
guidelines that will help future contributors
and agents understand the project's architecture, coding standards, and best
practices. In particular, consider adding:

- solutions to repeated issues (e.g, using the wrong flags for command-line
  tools)

## Build Prerequisites

Before building the project, ensure the following are installed and configured:

- **Node.js** and **Corepack** - Required for frontend builds. Enable corepack
  with: `corepack enable`
- **.NET 10.0 SDK** - Required for backend builds

## Architecture

### Core Components

- **Zylance.Core** - Core business logic with controllers, services, and Gateway
  for message routing
- **Zylance.Desktop** - Desktop application using Photino.NET for native
  windowing
- **Zylance.UI** - React + TypeScript + Vite frontend
- **Zylance.Contract** - Protocol Buffers message contracts for type-safe
  communication
- **Zylance.Vault.Local** - Local vault implementation using Entity Framework
  Core
- **Zylance.SourceGenerators** - Source generators for automatic controller
  registration
- **Docs** - Project documentation including format specifications and design
  documents

### Key Patterns

- **Dependency Injection**: Uses `Microsoft.Extensions.DependencyInjection`
  throughout
- **Gateway Pattern**: Central message router (`Gateway.cs`) handles
  request/response and events
- **Controller Pattern**: Controllers handle specific domains (File, Vault,
  Status, Echo)
- **Provider Pattern**: Platform-specific implementations via `ITransport`,
  `IFileProvider`, `IVaultProvider`

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

**Why?** Pattern matching (`is null`/`is not null`) is more consistent with
modern C# patterns, provides better type
narrowing, and is the preferred style in C# 9+.

### DTOs and POCOs

✅ **Prefer `record` types over `class` for data transfer objects and plain data
structures:**

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

- Allow object initialization syntax while preventing mutation after
  construction
- Make data immutability explicit and enforced by the compiler
- Reduce bugs from unintended state changes
- Work well with `required` keyword for mandatory properties
- Better express that DTOs/POCOs are immutable data contracts

### Regex Patterns

✅ **Use `[GeneratedRegex]` for simple patterns, `Lazy<Regex>` for complex ones:
**

```csharp
// Good - simple pattern with source generation
[GeneratedRegex(@"^\<(?'Name'[\w\d\.]+)\>$")]
private static partial Regex ElementStartRegex();

// Good - complex pattern built with string.Join for readability
private readonly static Lazy<Regex> DateTimeRegex = new(() =>
{
    var pattern = string.Join(
        "",
        @"(?'Year'\d{4})",
        @"(?'Month'\d{2})",
        @"(?'Day'\d{2})"
    );
    return new Regex($"^{pattern}$");
});
```

**Why?**

- Source-generated regex is fastest for simple patterns
- Complex patterns benefit from builder pattern for maintainability
- Add a comment explaining why you're using `Lazy<Regex>` instead of
  `[GeneratedRegex]`

### Naming Conventions

✅ **Use full, descriptive variable names - do NOT shorten:**

```csharp
// Good
var statementTransactionResponse = element.GetChild("STMTTRNRS");
var bankAccountFromElement = element.GetChild("BANKACCTFROM");
var datePostedToken = element.Tokens["DTPOSTED"];

// Avoid - shortened names
var stmtTrnRs = element.GetChild("STMTTRNRS");
var bankAcctFromElem = element.GetChild("BANKACCTFROM");
var dtPostedTok = element.Tokens["DTPOSTED"];
```

**Why?** Full names improve readability and make code self-documenting. Modern
IDEs handle autocomplete, so verbosity is not a burden.

### Extension Methods

✅ **Use C# 14 extension blocks for cleaner extension method definitions:**

```csharp
// Good - C# 14 extension block syntax
namespace MyNamespace;

public static class DateTimeOffsetExtensions
{
    extension(DateTimeOffset dateTime)
    {
        public string ToIso8601()
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        }
    }
}

// Traditional syntax (still valid, but extension blocks are preferred)
public static class DateTimeOffsetExtensions
{
    public static string ToIso8601(this DateTimeOffset dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
    }
}
```

**Why?** C# 14's extension blocks provide:

- Cleaner syntax without repetitive `this` keywords
- Groups related extension methods logically by the type they extend
- More readable when defining multiple extensions for the same type
- The target type (e.g., `DateTimeOffset`) is explicit in the `extension()`
  declaration

### Code Formatting

✅ **Always run CSharpier before committing:**

```bash
dotnet csharpier .
```

**Why?** Consistent formatting across the codebase improves readability and
reduces diff noise. The CI pipeline will fail if code is not properly formatted.

### Additional Guidelines

- Use `required` keyword for mandatory properties on records/classes
- Leverage source generators for repetitive code (see
  `Zylance.SourceGenerators`)
- Follow async/await patterns for I/O operations
- Use nullable reference types (`string?`) to express nullability explicitly
- Controllers should be stateless and rely on injected services
- Make internal classes testable via
  `[assembly: InternalsVisibleTo("ProjectName.Tests")]` in
  `Properties/AssemblyInfo.cs`

### Exception Classes

✅ **Use primary constructor pattern for exception classes:**

```csharp
// Good - primary constructor pattern
public class NonZylanceDatabaseException(string filePath, string reason)
    : Exception($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}")
{
    public string Reason { get; } = reason;
}

// Avoid - traditional constructor
public class NonZylanceDatabaseException : Exception
{
    public NonZylanceDatabaseException(string filePath, string reason)
        : base($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}")
    {
        Reason = reason;
    }
    
    public string Reason { get; }
}
```

**Why?** Primary constructors are more concise and consistent with modern C#
patterns (C# 12+). They reduce boilerplate while maintaining readability.

### Async Methods and Cancellation Tokens

✅ **Always include CancellationToken parameter in async methods:**

```csharp
// Good - includes cancellation token with default
public interface IMetadataManager
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
}

// Avoid - missing cancellation token
public interface IMetadataManager
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value);
}
```

**Why?** Cancellation tokens allow:

- Responsive cancellation of long-running operations
- Better resource management
- Improved user experience in UI applications
- Standard pattern for all async operations

### Entity Framework Configuration

✅ **Prefer data annotations on entity classes over Fluent API when possible:**

```csharp
// Good - declarative attributes on entity
[Table("_zylance_")]
public class ZylanceMetadataEntity
{
    [Key]
    [MaxLength(255)]
    public required string Key { get; init; }
    
    [MaxLength(255)]
    public required string Value { get; set; }
}

// Avoid - Fluent API in OnModelCreating (unless complex relationships require it)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ZylanceMetadataEntity>(entity =>
    {
        entity.ToTable("_zylance_");
        entity.HasKey(e => e.Key);
    });
}
```

**Why?** Data annotations:

- Keep configuration close to the entity definition
- Are easier to discover and maintain
- Make the entity's database mapping immediately visible
- Fluent API should be reserved for complex relationships and configurations
  that can't be expressed with attributes

### Comments and Documentation

✅ **Comments should explain *why*, not *what*:**

```typescript
// Bad - explains what the code does
// Loop through all files and process them
for (const file of files) {
  processFile(file);
}

// Good - explains why we're doing this
// Process files sequentially to avoid overwhelming the file system
for (const file of files) {
  processFile(file);
}

// Better - use well-named functions to make code self-documenting
async function processFilesSequentially(files: string[]) {
  // Sequential processing prevents file system overload
  for (const file of files) {
    await processFile(file);
  }
}
```

**Why?** Code should be self-documenting through clear naming. Comments add
value by explaining:

- Business logic decisions
- Performance considerations
- Workarounds for bugs or limitations
- Complex algorithms that aren't immediately obvious

Use descriptive function and variable names to convey *what* the code does,
reserving comments for *why* decisions were
made.

## Testing Guidelines

### Test Organization

✅ **Use xUnit theory tests with inline data for parameterized testing:**

```csharp
// Good - concise and readable test data
[Theory]
[InlineData("20220101123000", "2022-01-01T12:30:00+00:00")]
[InlineData("20231215083045", "2023-12-15T08:30:45+00:00")]
public void Parse_ValidInput_ParsesCorrectly(string input, string expected)
{
    var result = Parser.Parse(input);
    Assert.Equal(expected, result);
}

// Avoid - separate test methods for similar cases
[Fact]
public void Parse_FirstCase_Works() { /* ... */ }

[Fact]
public void Parse_SecondCase_Works() { /* ... */ }
```

### Test Naming

✅ **Use descriptive test names that indicate: Method_Scenario_ExpectedResult:**

```csharp
// Good
TryParse_ValidInput_ParsesCorrectly
TryParse_InvalidInput_ReturnsFalse
GetChildElement_MissingChild_ThrowsException

// Avoid
Test1
ParseTest
TestParser
```

### Test Coverage

Ensure tests cover:

- **Happy path** - valid inputs with expected outputs
- **Edge cases** - boundary conditions, special values
- **Error cases** - invalid inputs, null handling, exceptions
- **Format variations** - different valid input formats where applicable

**Why?** Well-organized parameterized tests are easier to maintain and extend.
Clear test names make failures immediately
understandable.

### xUnit Analyzer (xUnit1051)

✅ **Pass `TestContext.Current.CancellationToken` to async calls in tests when
a `CancellationToken` overload exists:**

```csharp
// Good
var cancellationToken = TestContext.Current.CancellationToken;
await connection.OpenAsync(cancellationToken);
await command.ExecuteReaderAsync(cancellationToken);
await vault.Metadata.SetAsync("version", "1.0.0", cancellationToken);

// Avoid - omits test cancellation token
await connection.OpenAsync();
```

**Why?** This keeps tests responsive to cancellation and avoids xUnit1051
warnings.

## Technology Stack

- **.NET 10.0** - Target framework
- **Photino.NET** - Native desktop windowing
- **React + TypeScript** - Frontend UI
- **Vite** - Frontend build tool
- **Protocol Buffers** - Serialization format
- **Entity Framework Core** - Database ORM (Local vault)
- **Roslyn Source Generators** - Code generation

### Key Libraries & Tools

#### Backend (.NET)

**Microsoft.Extensions.DependencyInjection** (10.0.2+)

- Built-in .NET dependency injection container
- Used throughout the application for service registration and resolution
- All controllers, services, and providers are registered via DI

**Microsoft.EntityFrameworkCore** (10.0.2+)

- ORM for database access in `Zylance.Vault.Local`
- Handles migrations, change tracking, and LINQ queries
- SQLite provider used for local vault storage

**Photino.NET**

- Cross-platform desktop application framework
- Provides native windowing with embedded web view
- Lightweight alternative to Electron - uses OS native WebView
- Used in `Zylance.Desktop` project

**Protocol Buffers (protobuf-net or Google.Protobuf)**

- Binary serialization format for efficient communication
- Type-safe contracts defined in `Zylance.Contract`
- Used for communication between UI and backend via custom transport layer

#### Frontend (React/TypeScript)

**React 19+**

- Component-based UI library
- Located in `Zylance.UI/Src`

**TypeScript**

- Type-safe JavaScript with compile-time checks
- All frontend code uses strict TypeScript

**Vite**

- Fast frontend build tool and dev server
- Hot module replacement (HMR) for development
- Configured in `Zylance.UI/vite.config.ts`

**Biome**

- Fast linter and formatter for JavaScript/TypeScript
- Replaces ESLint and Prettier
- Configuration in `Zylance.UI/biome.json`

#### Testing

**xUnit v3** (3.2.2+)

- Modern testing framework for .NET
- Uses `Microsoft.Testing.Platform` v2 (not the old test runners)
- Theory tests with `[InlineData]` for parameterized testing
- Test projects: `Zylance.Core.Tests`, `Zylance.Vault.Local.Tests`, etc.

**Important:** xUnit v3 uses different CLI arguments than v2:

```bash
# Use --filter-class, --filter-method, --filter-namespace
dotnet test --filter-class "*DateTimeOffsetParserTests"

# NOT the old --filter syntax
```

#### Development Tools

**Roslyn Source Generators** (`Zylance.SourceGenerators`)

- Compile-time code generation
- Automatically generates controller registration code
- Must be referenced as `Analyzer` in project references
- Generated files output to `obj/` when `EmitCompilerGeneratedFiles` is enabled

**JetBrains Rider / Visual Studio**

- Primary IDEs for development
- `.sln.DotSettings` files contain team-shared settings

### Common Patterns in Libraries

**Lazy<T>**

- Thread-safe lazy initialization
- Used for expensive resources like compiled Regex patterns
- Initialized only once on first access

**Source-Generated Regex** (`[GeneratedRegex]`)

- Compile-time regex generation (C# 11+)
- Faster than runtime-compiled regex
- Requires `partial` class/method
- Use for simple patterns; fall back to `Lazy<Regex>` for complex patterns

**InternalsVisibleTo**

- Makes `internal` classes visible to test projects
- Defined in `Properties/AssemblyInfo.cs`
- Enables testing of internal implementation details

