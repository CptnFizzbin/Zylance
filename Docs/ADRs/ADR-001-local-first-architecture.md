# ADR-001: Local-First Architecture with Multi-Platform Runtime Flags

## Context

Finance and budgeting applications handle sensitive personal data that users rightfully want to keep private and accessible. Many modern apps force users into cloud-first architectures where data immediately syncs to remote servers, creating privacy concerns and vendor lock-in. Additionally, cross-platform applications often maintain separate codebases for each platform, leading to feature fragmentation and increased maintenance burden.

For Zylance v1, we needed to decide:
1. Should data be stored locally or in the cloud by default?
2. How should we architect for future multi-platform support (desktop, web, mobile)?
3. Should we use compile-time platform separation or runtime detection?

## Decision

Zylance will use a **local-first architecture** for v1, where all user data is stored locally on the device by default. No cloud sync or remote storage is required for core functionality.

For multi-platform support, we will use **runtime flags** (`IsDesktop`, `IsWeb`, `IsMobile`) rather than separate platform-specific implementations. The same codebase and UI components will run across all platforms, with platform-specific behavior handled through conditional logic based on these flags.

This means:
- Desktop, web, and mobile will share the same React/TypeScript UI codebase (`Zylance.UI`)
- The same business logic layer (`Zylance.Core`) serves all platforms
- Platform-specific features (like file system access) are abstracted through provider interfaces (`IFileProvider`, `IVaultProvider`)
- Platform detection happens at runtime, not compile time
- Feature parity is prioritized—all platforms get the same features wherever possible

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

- **Runtime overhead**: Platform checks add small runtime cost vs. compile-time separation
- **Bundle size**: All platform code ships to all platforms (though this is minimal)
- **Sync complexity**: When remote sync is added later, it becomes an opt-in feature rather than a first-class design
- **Initial limitations**: Some platform-specific optimizations may be harder to implement
- **Conditional complexity**: Runtime flags can lead to complex conditional logic if not managed carefully

### Mitigations

- Use clear provider abstractions to isolate platform-specific code
- Establish coding conventions for how to use runtime flags cleanly
- Consider code-splitting strategies if bundle size becomes an issue
- Design remote sync as an enhancement to local-first, not a replacement

## General Notes

This decision reflects a philosophical stance on data ownership and user privacy. By making local-first the default, we're saying that users should own their financial data without being forced into cloud storage.

The multi-platform runtime flag approach was influenced by modern web frameworks like React Native and Electron, which successfully run the same codebase across platforms. While this adds some runtime checks, the development velocity gains and consistency benefits far outweigh the minor performance cost.

The key insight is that most finance app features are platform-agnostic—budget tracking, transaction categorization, and reporting work the same everywhere. Only a small subset of features (file system access, biometric auth, native notifications) truly need platform-specific implementations.

This approach also positions us well for WASM. A web version can run the same UI and business logic, with providers that use browser APIs instead of native system calls.

**For future blog post**: Could explore the tension between "one codebase" and "platform-specific optimization." The runtime flag approach is a pragmatic middle ground that prioritizes developer experience and user consistency over theoretical purity.
