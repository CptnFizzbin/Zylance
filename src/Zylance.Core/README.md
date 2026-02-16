# Zylance.Core

The primary package that contains all the business logic for the Zylance
application.

## Overview

Zylance.Core implements the core functionality of the application using a clean
architecture approach. It handles message routing, business logic processing,
and coordinates between the UI layer and vault providers.

## Key Components

### Gateway

Central message router that handles request/response communication and events
between the UI and business logic.

### Controllers

Domain-specific controllers that process requests:

- **FileController** - File operations and management
- **VaultController** - Vault lifecycle management
- **StatusController** - Application status and health
- **EchoController** - Message echo for testing/debugging

### Services

Business logic services that implement the core functionality of the
application.

### Providers

Platform-agnostic interfaces for external dependencies:

- **IFileProvider** - File system operations
- **IVaultProvider** - Vault implementation provider

## Architecture

The Core follows these principles:

- **Dependency Injection** - Uses `Microsoft.Extensions.DependencyInjection`
- **Gateway Pattern** - Central routing for all messages
- **Controller Pattern** - Domain-driven request handlers
- **Provider Pattern** - Platform-specific implementations via interfaces

## Dependencies

- Protocol Buffers (via `Zylance.Contract`)
- Microsoft.Extensions.DependencyInjection
- Source generators for automatic controller registration

## Usage

The Core is consumed by platform-specific hosts (e.g., `Zylance.Desktop`) which
provide concrete implementations of the provider interfaces and wire up the
Gateway for communication with the UI.

## Glossary

- **Data**: Data transfer object for communication between Core and the UI (see:
  `*Data` types, e.g., `AccountData`).
- **Model**: Internal object for Core to operate on (see: `*Model` types, e.g.,
  `AccountModel`).
- **Entity**: Database objects used internally by Vault implementations (see:
  `*Entity` types, e.g., `AccountEntity`).
