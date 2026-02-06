# ADR-008: Single UI Codebase Across All Platforms

## Context

Cross-platform applications traditionally face a difficult choice:
1. **Native per platform**: Build separate UIs for each platform (iOS, Android, Desktop, Web)
2. **Lowest common denominator**: Build for the most limited platform, sacrificing features
3. **Platform detection at compile time**: Separate builds with platform-specific code
4. **Single codebase with runtime adaptation**: One UI that adapts to the platform

Each approach has trade-offs:
- **Native per platform**: Maximum platform optimization, but massive duplication and feature fragmentation
- **Lowest common denominator**: Consistency, but poor experience on capable platforms
- **Compile-time separation**: Some code sharing, but still requires separate maintenance
- **Runtime adaptation**: Maximum code sharing, but complexity in conditional logic

Zylance targets multiple platforms (desktop first, web and mobile later). We needed to decide how to structure the UI codebase to:
1. Maximize code reuse across platforms
2. Maintain feature parity (same features everywhere)
3. Allow platform-specific optimizations where needed
4. Keep development velocity high

## Implementation

**Status**: Complete

## Decision

Use a **single UI codebase** (`Zylance.UI`) built with React + TypeScript + Vite that runs on **all platforms** (desktop, web, mobile), with **runtime platform detection** and conditional rendering for platform-specific behavior.

This means:
1. **One React/TypeScript codebase**: Same components, same state management, same business logic
2. **Runtime flags**: Platform detection via `IsDesktop`, `IsWeb`, `IsMobile` flags
3. **Conditional rendering**: Platform-specific UI variations handled via conditional logic
4. **Provider pattern**: Platform-specific features (file system, notifications) abstracted behind interfaces
5. **Feature parity**: All platforms get all features unless physically impossible
6. **No platform-specific builds**: Single build artifact runs everywhere (with minor bundling differences)

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

The UI project structure:
```
Zylance.UI/
├── Src/
│   ├── Routes/          # Route components
│   ├── Components/      # Shared components
│   ├── Integrations/    # Third-party integrations
│   └── Hooks/           # Custom React hooks
├── Public/              # Static assets
├── package.json
└── vite.config.ts
```

## Consequences

### Positive

- **Maximum code reuse**: 90%+ of UI code is shared across platforms
- **Feature parity**: Same features on all platforms by default
- **Consistent UX**: Users get the same experience regardless of device
- **Faster development**: Write once, run everywhere
- **Easier testing**: One test suite covers all platforms
- **Simplified maintenance**: Bug fixes apply to all platforms
- **Designer-friendly**: Design once, deploy everywhere
- **Cross-platform state**: Business logic doesn't care about platform

### Negative

- **Runtime overhead**: Platform checks add small performance cost
- **Bundle size**: All platform code ships to all platforms (though minimal)
- **Conditional complexity**: Too many platform checks can make code hard to read
- **Testing complexity**: Must test all platform branches
- **Platform-specific quirks**: Still need to handle browser vs. WebView differences
- **Optimization limits**: Can't fully optimize for any single platform
- **CSS challenges**: Styling differences between platforms require careful handling

### Mitigations

- Use well-named utility functions to hide platform checks
- Establish patterns for platform-specific components
- Use code splitting to avoid shipping unnecessary code
- Implement platform-specific providers for true platform features
- Create platform-specific CSS modules when needed
- Write integration tests that simulate different platforms
- Document platform-specific behavior clearly
- Consider dynamic imports for large platform-specific features

## General Notes

This decision aligns with modern cross-platform frameworks like React Native, Flutter, and Electron. The insight is that **most UI features are platform-agnostic**. Buttons, forms, lists, charts—these work the same everywhere. Only a small subset of features truly need platform-specific implementations.

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

The provider pattern helps encapsulate platform-specific features. For example:
```tsx
interface IFileProvider {
  pickFile(): Promise<File>;
  saveFile(data: Blob, name: string): Promise<void>;
}

class DesktopFileProvider implements IFileProvider { /* ... */ }
class WebFileProvider implements IFileProvider { /* ... */ }
```

This keeps platform-specific code isolated and testable.

**Comparison with other approaches:**

**React Native:**
- Also uses single codebase with runtime platform detection
- More mature ecosystem but heavier runtime
- Not chosen because we wanted to use familiar web tools like MUI (Material-UI)
- Unfamiliarity with React Native's design flexibility constraints

**Electron:**
- Single codebase, but desktop-only
- Not chosen because it was too heavy of an engine
- Photino is similar to Rust's Tauri project: provides lightweight wrappers around native web views
- Cross-platform with significantly smaller footprint

**The bundle size concern:**
The worry about shipping all platform code to all platforms is mostly theoretical. Platform-specific code is tiny compared to the shared business logic and UI components. With code splitting and tree-shaking, the actual overhead is negligible.

**Developer experience:**
One codebase means:
- New team members learn one stack
- Changes are tested once and work everywhere
- Debugging is simpler (reproduce on any platform)
- Deployment pipeline is simpler

**Evolution path:**
If we discover a platform needs significantly different UI (unlikely), we can refactor to platform-specific component trees while still sharing business logic. The architecture doesn't lock us in—it's a starting point that can evolve.

**Inspiration from web history:**
This approach is similar to responsive web design. Instead of separate mobile and desktop sites, you build one site that adapts. That worked so well for the web that it became the standard. We're applying the same principle to cross-platform apps.

**Testing strategy:**
We test the UI once but with different platform configurations:
```tsx
describe('FileImportButton', () => {
  it('renders native picker on desktop', () => {
    render(<FileImportButton />, { platform: 'desktop' });
    // assertions
  });
  
  it('renders web input on web', () => {
    render(<FileImportButton />, { platform: 'web' });
    // assertions
  });
});
```

This gives us confidence that all platform branches work correctly.

**For future blog post**: Could explore the "Write Once, Run Everywhere" promise that has been pursued for decades:
- Java's original promise (didn't quite deliver)
- Web's success with responsive design
- React Native's approach and limitations
- Our take: runtime platform detection in React
- When single codebase makes sense vs. when platform-specific is better
- The 95/5 rule: 95% shared, 5% platform-specific
- Case study: Real examples from Zylance showing the pattern in action
