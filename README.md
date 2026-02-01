# Zylance

An OpenSource finance and budgeting app

> # Work in Progress
> This project is in early development, with almost no features implemented yet.
> Expect dragons and incomplete functionality.

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
4. Install Node.js and Yarn using FNM (or your preferred Node.js version manager):
   ```pwsh
   fnm install 24.5.0
   fnm use 24.5.0
   corepack enable
   corepack prepare yarn@stable --activate
   ```
5. Set the `NODE_HOME` environment variable to your Node.js installation directory:
   > **Note:** The build process uses `NODE_HOME` to locate yarn. Adjust the path based on your Node.js installation
   method.

   **Windows (PowerShell):**
   ```pwsh
   # For FNM users - set in current session:
   $env:NODE_HOME = "$env:APPDATA\fnm\node-versions\v24.5.0\installation"
   
   # Or set permanently (requires restart):
   [System.Environment]::SetEnvironmentVariable('NODE_HOME', "$env:APPDATA\fnm\node-versions\v24.5.0\installation", 'User')
   ```

   **Linux/macOS:**
   ```bash
   # For FNM users - add to your shell profile (~/.bashrc, ~/.zshrc, etc.):
   export NODE_HOME="$HOME/.local/share/fnm/node-versions/v24.5.0/installation"
   ```
6. build the solution:
   ```pwsh
   dotnet build
   ```

### Code Formatting

This project uses [CSharpier](https://csharpier.com/) for consistent C# code formatting.

**First-time setup:**

```bash
dotnet tool install csharpier
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

