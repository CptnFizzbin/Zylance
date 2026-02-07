# ADR-000: Technology Stack

## Context

Zylance is a personal finance and budgeting application built from the ground up with a focus on learning modern development practices while maintaining productivity. The technology stack needed to support multiple platforms (desktop with future web/mobile support) while balancing learning opportunities with familiar tools.

The overarching philosophy for technology choices is: **"Limit new tool learning to strategic areas; use familiar tools for familiar territory."**

This approach means:
- Using C# and .NET despite being relatively new to the developer, because the language and ecosystem are impressive and worth learning
- Using familiar patterns (Spring Boot-style dependency injection) for backend infrastructure where concepts transfer
- Using familiar UI frameworks (Material-UI) where possible to reduce cognitive load
- Concentrating the learning curve on genuinely new areas (TanStack ecosystem, Photino.NET, C# idioms)
- Avoiding complex paradigm shifts (e.g., Rust's borrow checker) while actively learning multiple systems
- Choosing "boring" but stable technologies over cutting-edge options
- Preferring widely-known tools that won't become unmaintained

This prevents overwhelming the developer with too many new concepts simultaneously while still allowing strategic learning in areas of interest (C# ecosystem, modern web frameworks, desktop application development).

## Implementation

**Status**: N/A - Reference Document

## Decision

### Backend/Core Technology

**C# and .NET 10.0**
- Chosen for learning purposes and personal history
- The developer's father is a C# developer, and C# was their first programming language in high school, making this a "full circle" experience
- Modern language features (records, pattern matching, nullable reference types) impressed the developer
- Strong type system and tooling provide safety nets during learning
- Extensive ecosystem and community support

**Entity Framework Core v10.0.2**
- Chosen as the most widely-known and widely-used ORM in the .NET ecosystem
- Standard choice for .NET projects with excellent documentation
- Used only for local vault implementation (`Zylance.Vault.Local`)
- Provides migrations, change tracking, and LINQ queries out of the box

**Microsoft.Extensions.DependencyInjection**
- Familiar from Spring Boot patterns in the developer's day job
- Used for dependency injection and service management throughout the application
- All controllers, services, and providers registered via DI
- Clean separation of concerns and testability

### Frontend Technology

**React 19 + TypeScript**
- Type-safe UI development across multiple platforms
- React provides component reusability and excellent developer experience
- TypeScript adds compile-time safety and better IDE support
- Strong ecosystem and community

**Material-UI (MUI)**
- Excellent theming support for consistent design system
- The `sx` prop provides inline styling flexibility when needed
- Developer familiarity with the library reduces learning curve
- Comprehensive component library reduces custom UI code

**TanStack Suite**
- **TanStack Router**: File-based routing system for intuitive project structure
- **TanStack Query**: Async caching capabilities for data management
- **TanStack Form**: Usability and composability for form handling
- **TanStack Table**: New library to learn as part of the project
- Superior TypeScript support and ease of use compared to alternatives
- Modern, well-maintained libraries with active communities

**Vite**
- Significantly faster build times compared to Webpack
- Excellent Hot Module Replacement (HMR) support for development productivity
- Easier to set up and use with Node scripts and TypeScript
- Modern, ESM-first architecture
- Configured in `Zylance.UI/vite.config.ts`

**Biome**
- All-in-one linting and formatting solution
- Replaces ESLint + Prettier + other tools with single dependency
- Fast, modern tool built in Rust
- Reduces configuration complexity
- Configuration in `Zylance.UI/biome.json`

**Vitest**
- Testing framework familiar from BDD-style test frameworks like RSpec
- Seamless integration with Vite
- Fast test execution with watch mode
- TypeScript support out of the box

### Desktop/Platform

**Photino.NET v4.0.16**
- Lightweight cross-platform desktop framework
- Chosen after exploring multiple options suggested by Copilot
- Seemed like the only option that truly fit the project needs without excessive complexity
- Chosen over Electron for its lightweight nature (native WebView instead of bundled Chromium)
- Chosen over Tauri (Rust-based) to avoid Rust's borrow checker learning curve while already learning multiple systems
- Documentation is extremely light and somewhat painful to work with (acknowledged limitation)
- May be re-evaluated in the future if better options emerge or if documentation/community improves
- Native windowing with OS-provided WebView keeps bundle size small

### Communication Layer

**Protocol Buffers (protobuf)**
- Type-safe client-server communication across the TypeScript/C# boundary
- Schema-first approach with `.proto` files defining contracts
- Both languages generate code from the same definitions
- Compile-time checking prevents API mismatches
- See ADR-007 for detailed rationale

**ts-proto**
- TypeScript code generator for Protocol Buffers
- Generates clean, idiomatic TypeScript types from `.proto` files
- Integrates well with the frontend build process

**Google.Protobuf**
- C# runtime library for Protocol Buffers
- Standard .NET implementation with excellent support
- Handles serialization/deserialization in the backend

### Testing

**xUnit v3.2.2**
- .NET standard for greenfield projects
- Chosen over NUnit/MSTest as it's the industry standard
- Modern testing framework with excellent tooling
- Theory tests with `[InlineData]` for parameterized testing
- Uses `Microsoft.Testing.Platform` v2 (not the old test runners)
- Test projects: `Zylance.Core.Tests`, `Zylance.Vault.Local.Tests`, etc.

**Vitest** (covered above in Frontend)
- TypeScript/JavaScript testing with familiar BDD patterns
- Used for frontend React component and logic testing

### Build & Development

**Yarn 4.12.0**
- Preferred Node package manager
- Modern features (Plug'n'Play, zero-installs)
- Better dependency management than npm
- Consistent with current JavaScript ecosystem practices

**MSBuild Integration**
- `.csproj` files orchestrate both Node.js and C# builds
- Unified development workflow across frontend and backend
- Single `dotnet build` command builds entire application
- Simplified CI/CD pipeline

**Roslyn Source Generators** (`Zylance.SourceGenerators`)
- Compile-time code generation for repetitive tasks
- Automatically generates controller registration code
- Reduces boilerplate and potential for human error
- See ADR-005 for detailed rationale

## Consequences

### Positive

- **Strategic learning**: Focused learning on C# and .NET while using familiar patterns (DI, component-based UI)
- **Type safety end-to-end**: TypeScript on frontend, C# on backend, Protocol Buffers for communication
- **Modern tooling**: Fast build times (Vite), excellent IDE support (Roslyn, TypeScript), modern frameworks
- **Lightweight desktop**: Photino.NET provides native windowing without Electron's overhead
- **Unified build**: MSBuild integration means one command builds both frontend and backend
- **Familiar where it counts**: DI patterns from Spring Boot, BDD testing patterns, Material-UI components reduce cognitive load
- **Strong ecosystems**: All major choices (.NET, React, MUI, TanStack) have large communities and extensive documentation
- **Future-proof**: Technologies chosen are actively maintained and widely adopted
- **Personal significance**: C# choice has personal meaning (father's language, first language learned), making the learning journey more meaningful
- **Cross-platform foundation**: Stack supports future web and mobile platforms without major rewrites

### Negative

- **Photino.NET documentation**: Light documentation can make some tasks more difficult than necessary
- **Multiple new concepts**: Learning C# idioms, .NET patterns, and modern C# features simultaneously
- **Build complexity**: Integration between Node.js (Vite) and MSBuild adds some complexity
- **Protobuf learning curve**: Team must understand protobuf syntax and code generation process
- **TanStack learning**: Multiple new TanStack libraries to learn (Router, Query, Form, Table)
- **Biome adoption**: Newer tool with smaller community than ESLint/Prettier combination
- **xUnit v3 differences**: Different CLI arguments than xUnit v2, must use `--filter-class` instead of `--filter`

### Mitigations

- **Documentation**: Create internal documentation for Photino.NET usage patterns as they're discovered
- **Incremental learning**: Focus on one new technology deeply before moving to the next
- **Copilot assistance**: Use GitHub Copilot for productivity when working with less familiar technologies (see ADR-006)
- **Community resources**: Leverage Discord communities, Stack Overflow, and official documentation
- **Code reviews**: Regular self-review and reflection on code quality and patterns
- **Testing**: Comprehensive test coverage provides safety net when learning new patterns
- **Fallback plans**: Re-evaluate Photino.NET if it becomes a significant blocker; Tauri or Electron remain options

## General Notes

This technology stack reflects a deliberate balance between learning and productivity. The choices prioritize:

1. **Learning with guard rails**: C# and .NET are new but provide strong typing and tooling to catch mistakes early
2. **Familiar patterns**: DI, component-based UI, and BDD testing patterns transfer from previous experience
3. **Strategic novelty**: New learning focused on valuable, transferable skills (modern C#, TanStack ecosystem)
4. **Avoiding complexity traps**: Steering clear of Rust's borrow checker while already learning multiple systems
5. **Industry standards**: Choosing widely-adopted tools (xUnit, Entity Framework Core, React) over niche alternatives
6. **Personal meaning**: The C# choice connects the developer's past (first language, father's expertise) with present learning goals

**Why C# and .NET specifically:**
The decision to use C# despite being relatively new to the developer is intentional. The language's modern features (records, pattern matching, init-only properties, nullable reference types) and the .NET ecosystem's maturity impressed the developer. The personal connection (father's language, first high school language) adds emotional resonance to the learning journey, making it a "full circle" moment. The strong typing and excellent tooling (Roslyn, Rider) provide safety nets that make learning less intimidating.

**Why Entity Framework Core:**
As the most widely-known ORM in .NET, Entity Framework Core is the "boring" choice - and that's exactly why it's right. It has extensive documentation, huge community, and won't become unmaintained. For a learning project, choosing the standard tool means more resources when stuck and more transferable knowledge.

**Why Material-UI over alternatives:**
While libraries like Chakra UI or Ant Design have their merits, Material-UI's combination of excellent theming, the flexible `sx` prop, and developer familiarity made it the pragmatic choice. The theming system in particular enables consistent design without fighting the framework.

**Why TanStack over alternatives:**
The TanStack suite (formerly React Query, React Router, etc.) provides superior TypeScript support and modern API design compared to alternatives. Each library in the suite is focused and composable, and the ecosystem is well-maintained. TanStack Table specifically was chosen as a learning opportunity - a new library to master as part of the project.

**Why Photino.NET despite documentation concerns:**
After exploring options, Photino.NET emerged as the best fit despite its documentation limitations. Electron's bundle size and resource usage were deal-breakers. Tauri would require learning Rust's borrow checker on top of everything else. Photino.NET's lightweight approach (using OS WebView) and .NET integration made it worth the documentation trade-off. The project can always pivot if it becomes a blocker.

**Why Vitest over Jest:**
Vitest's seamless Vite integration, familiar BDD syntax, and modern architecture made it the natural choice for a Vite-based project. The developer's familiarity with RSpec-style testing patterns (describe/it blocks) transfers directly.

**Philosophy in practice:**
The stack demonstrates the "limit new learning to strategic areas" philosophy:
- **New strategic learning**: C# language, .NET ecosystem, TanStack libraries, Photino.NET
- **Familiar territory**: DI patterns, component architecture, BDD testing, Material-UI
- **Avoided complexity**: Rust, cutting-edge experimental tools, custom serialization formats

This approach has proven effective: the developer can focus cognitive effort on learning C# deeply while relying on familiar patterns elsewhere. When stuck on a C# problem, the familiar frontend stack provides a comfortable place to make progress.

**Lessons learned:**
1. **Boring is good**: Choosing standard tools (EF Core, xUnit, Material-UI) means better documentation and community support
2. **Personal meaning matters**: The C# "full circle" narrative makes learning more engaging
3. **Transfer your patterns**: DI experience from Spring Boot directly informed the .NET architecture
4. **Accept trade-offs**: Photino.NET's documentation is painful, but the alternative (Electron bloat or Rust complexity) is worse
5. **One new thing deeply**: Focus on learning C# deeply rather than surface-level knowledge of many tools

**For future blog post**: Could write about "Strategic Learning in Side Projects" covering:
- How to balance learning new technologies with shipping features
- The "limit new learning" philosophy and why it works
- Why "boring" technology choices enable deeper learning in strategic areas
- The value of emotional connection to technology choices (the "full circle" narrative)
- How to evaluate when to learn something new vs. stick with familiar tools
- Avoiding the trap of learning too many things at once and burning out
