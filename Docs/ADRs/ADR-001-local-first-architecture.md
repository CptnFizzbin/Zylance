# ADR-001: Local-First Architecture

## Context

Finance and budgeting applications handle sensitive personal data that users rightfully want to keep private and accessible. Many modern apps force users into cloud-first architectures where data immediately syncs to remote servers, creating privacy concerns and vendor lock-in.

For Zylance v1, we needed to decide:
1. Should data be stored locally or in the cloud by default?
2. How should we architect storage to support multiple vault providers?

## Implementation

**Status**: In Progress

## Decision

Zylance will use a **local-first architecture** for v1, where all user data is stored locally on the device by default. No cloud sync or remote storage is required for core functionality.

Data will be stored in an **encrypted SQLite database** (`.zlv` format - Zylance Vault). An option in the UI to toggle encryption off will be provided for:
- Development and debug purposes
- Allowing users to inspect what's stored by Zylance
- Preventing vendor lock-in and allowing for migrations

The architecture uses `IVaultProvider` and `IVault` interfaces, making **remote and other storage solutions first-class by default**. When the user opens the app, they'll be presented with the choice of provider (defaulting to Local).

This means:
- All user data is stored in an encrypted SQLite database by default
- The `IVaultProvider` abstraction allows easy swapping between Local, Remote, and other vault types
- Users maintain full control of their data
- Remote storage is a first-class option, not an afterthought

## Consequences

### Positive

- **Privacy by default**: Users maintain full control of their financial data without mandatory cloud sync
- **Offline-first**: The app works completely offline with no internet dependency
- **No vendor lock-in**: Users own their data in a local database they can access
- **Single codebase**: One UI implementation reduces development effort and ensures feature consistency
- **Platform flexibility**: New platforms can be added without rewriting UI components
- **Consistency**: Users get the same experience across all devices
- **Simplified testing**: One codebase means one set of tests (plus platform-specific provider tests)

### Negative

- **Initial limitations**: Some platform-specific optimizations may be harder to implement

### Mitigations

- Use clear provider abstractions (`IVaultProvider`, `IFileProvider`) to isolate storage concerns
- Provide encryption toggle for transparency and migration support

## General Notes

This decision reflects a philosophical stance on data ownership and user privacy. By making local-first the default, we're saying that users should own their financial data without being forced into cloud storage.

The use of encrypted SQLite by default provides security while maintaining the ability to inspect data when needed (via the encryption toggle). This transparency helps build user trust and prevents vendor lock-in.

The `IVaultProvider` abstraction is key to making remote storage a first-class feature. Users can choose their vault provider at app startup, with local being the default but remote being equally supported architecturally.

**For future blog post**: Could explore the tension between "privacy by default" and "convenience of cloud sync." The provider pattern approach lets users choose their preference without compromising the architecture.
