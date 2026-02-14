# Zylance.UI

The shared UI layer for Zylance, built with React and TypeScript. This UI will be reused across platforms in the future.

## Overview

Zylance.UI provides a modern, glassmorphic interface with an elegant gold and silver/gray theme. It communicates with the Core business logic via Protocol Buffers over a transport layer.

## Technology Stack

- **React** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server
- **Material-UI (MUI)** - Component library with custom theme
- **Protocol Buffers** - Type-safe communication with Core

## Theme

The UI features a sophisticated glassmorphism design:
- **Light Mode** - Gold and silver palette
- **Dark Mode** - Gold and gray palette
- Translucent backgrounds with backdrop blur effects
- Smooth transitions and hover effects

## Project Structure

```
Src/
  Components/     - Reusable UI components
  Routes/        - Page/route components
  Integrations/  - External integrations (MUI theme, etc.)
  Lib/           - Utility libraries and API client
Generated/       - Auto-generated TypeScript types from Protocol Buffers
Public/          - Static assets
```

## Communication Layer

The UI uses auto-generated TypeScript types from `Zylance.Contract` to ensure type-safe communication with the Core. Messages are serialized using Protocol Buffers and sent over the transport layer provided by the platform host.

## Development

```bash
yarn install
yarn start
```

## Building

```bash
yarn build
```

The build output is consumed by platform-specific hosts (e.g., `Zylance.Desktop`) for deployment.

## Platform Independence

While currently used by the Desktop application, this UI is designed to be reusable across future platforms (web, mobile, etc.) by abstracting the transport layer.
