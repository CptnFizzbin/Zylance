# Zylance

An OpenSource finance and budgeting app

## Architecture

Zylance uses a clean architecture with Dependency Injection for loose coupling and testability.

### Core Components

- **Zylance.Core** - Core business logic, controllers, and services
- **Zylance.Desktop** - Desktop application using Photino
- **Zylance.UI** - React/TypeScript frontend
- **Zylance.Contract** - Protocol Buffers message contracts
- **Zylance.Vault.Local** - Local vault implementation

### Dependency Injection

The application uses `Microsoft.Extensions.DependencyInjection` for service registration and resolution.

**Quick Start:**

```csharp
var services = new ServiceCollection();
services.AddSingleton<ITransport, MyTransport>();
services.AddSingleton<IFileProvider, MyFileProvider>();
services.AddSingleton<IVaultProvider, MyVaultProvider>();
services.AddZylance();

var serviceProvider = services.BuildServiceProvider();
var app = serviceProvider.GetRequiredService<Zylance>();
```

## Development

- Built with .NET 10.0
- Frontend: React + TypeScript + Vite
- Desktop: Photino.NET
- Communication: Protocol Buffers over custom transport layer

### Install

1. Clone the repository
2. Navigate to the project directory
3. Restore dependencies:
   ```pwsh
   dotnet restore
   ```
4. Install Node.js using FNM:
   ```pwsh
   fnm install 24.5.0
   fnm use 24.5.0
   corepack enable
   ```
5. Install frontend dependencies:
   ```pwsh
   cd Zylance.UI
   yarn install
   ```

### Code Formatting

This project uses [CSharpier](https://csharpier.com/) for consistent C# code formatting.

**First-time setup:**

```bash
dotnet tool restore
```

**Format all files:**

```bash
dotnet csharpier .
```

**Check formatting (CI/CD):**

```bash
dotnet csharpier --check .
```

**IDE Integration:**

- **Rider**: Install the "CSharpier" plugin from the marketplace and enable "Reformat with CSharpier on Save" in
  settings
- **VS Code**: Install the "CSharpier" extension
- **Visual Studio**: Install the "CSharpier" extension

