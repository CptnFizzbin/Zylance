# ADR-009: Runtime Platform Detection

## Context

Cross-platform applications often maintain separate codebases for each platform, leading to feature fragmentation and increased maintenance burden. Alternatively, they use compile-time platform separation which requires separate builds with platform-specific code.

Zylance targets multiple platforms (desktop first, web and mobile later). We needed to decide:
1. Should we use compile-time platform separation or runtime detection?
2. How do we handle platform-specific behavior?
3. How do we maintain feature parity across platforms?

## Implementation

**Status**: Complete

## Decision

Use **runtime flags** (`IsDesktop`, `IsWeb`, `IsMobile`) rather than separate platform-specific implementations. The same codebase and UI components will run across all platforms, with platform-specific behavior handled through conditional logic based on these flags.

This means:
- Desktop, web, and mobile will share the same React/TypeScript UI codebase (`Zylance.UI`)
- The same business logic layer (`Zylance.Core`) serves all platforms
- Platform-specific features (like file system access) are abstracted through provider interfaces (`IFileProvider`, `IVaultProvider`)
- Platform detection happens at runtime, not compile time
- Feature parity is prioritized—all platforms get the same features wherever possible

Example pattern:
```tsx
// Single component that works everywhere
function FileImportButton() {
  if (platform.isDesktop) {
    return <NativeFilePickerButton />;
  } else if (platform.isWeb) {
    return <WebFileInputButton />;
  } else {
    return <MobileFilePickerButton />;
  }
}
```

## Consequences

### Positive

- **Single codebase**: One UI implementation reduces development effort and ensures feature consistency
- **Platform flexibility**: New platforms can be added without rewriting UI components
- **Consistency**: Users get the same experience across all devices
- **Simplified testing**: One codebase means one set of tests (plus platform-specific provider tests)
- **Feature parity**: Same features on all platforms by default
- **Faster development**: Write once, run everywhere

### Negative

- **Runtime overhead**: Platform checks add small runtime cost vs. compile-time separation
- **Bundle size**: All platform code ships to all platforms (though this is minimal)
- **Conditional complexity**: Runtime flags can lead to complex conditional logic if not managed carefully
- **Platform-specific quirks**: Still need to handle browser vs. WebView differences
- **Testing complexity**: Must test all platform branches

### Mitigations

- Use well-named utility functions to hide platform checks
- Establish patterns for platform-specific components
- Use code splitting to avoid shipping unnecessary code
- Implement platform-specific providers for true platform features
- Create platform-specific CSS modules when needed
- Write integration tests that simulate different platforms
- Document platform-specific behavior clearly

## General Notes

The multi-platform runtime flag approach was influenced by modern web frameworks like React Native and Electron, which successfully run the same codebase across platforms. While this adds some runtime checks, the development velocity gains and consistency benefits far outweigh the minor performance cost.

The key insight is that most finance app features are platform-agnostic—budget tracking, transaction categorization, and reporting work the same everywhere. Only a small subset of features (file system access, biometric auth, native notifications) truly need platform-specific implementations.

**What's platform-agnostic (95% of the UI):**
- Budget views and transaction lists
- Charts and reports
- Settings screens
- Account management
- Category management
- Data visualization
- State management
- Business logic

**What's platform-specific (5% of the UI):**
- File system access (native vs. web file API)
- Biometric authentication (Touch ID vs. Web Authentication)
- Native notifications vs. web notifications
- App lifecycle events
- Deep linking / URL handling
- Platform-specific UI guidelines (minor styling differences)

This approach also positions us well for WASM. A web version can run the same UI and business logic, with providers that use browser APIs instead of native system calls.

**For future blog post**: Could explore the tension between "one codebase" and "platform-specific optimization." The runtime flag approach is a pragmatic middle ground that prioritizes developer experience and user consistency over theoretical purity. The 95/5 rule: 95% shared, 5% platform-specific.
