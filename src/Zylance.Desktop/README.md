# Zylance.Desktop

The Desktop edition of Zylance, providing a native windowing experience using Photino.NET.

## Overview

Zylance.Desktop is the desktop application host that brings together the Core business logic, UI layer, and platform-specific implementations for Windows, macOS, and Linux.

## Key Components

### PhotinoTransport
Implements the `ITransport` interface using Photino.NET for bidirectional communication between the native window and the React UI via Protocol Buffers.

### DesktopFileProvider
Desktop-specific implementation of `IFileProvider` for file system operations.

### DesktopVaultProvider
Desktop-specific implementation of `IVaultProvider` that manages vault instances and their lifecycle.

## Technology Stack

- **Photino.NET** - Cross-platform native windowing
- **Zylance.Core** - Business logic
- **Zylance.UI** - React/TypeScript frontend
- **Zylance.Contract** - Protocol Buffer message contracts

## How It Works

1. Bootstraps the Photino window with the compiled React UI
2. Initializes the Core with desktop-specific provider implementations
3. Sets up the PhotinoTransport for message passing between UI and Core
4. Manages the application lifecycle and native window events

## Building

The Desktop application automatically builds the UI project as part of its build process and embeds the static assets into the native application.

## Platform Support

- Windows
- macOS
- Linux
