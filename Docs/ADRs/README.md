# Architecture Decision Records (ADRs)

## Overview

This directory contains Architecture Decision Records (ADRs) for the Zylance project. ADRs document significant architectural decisions made during development, providing context and rationale for future maintainers and contributors.

## Format

We use a modified version of the [Michael Nygard format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) with the following structure:

- **Title**: A short, descriptive name for the decision
- **Context**: The circumstances and forces at play that led to this decision
- **Decision**: The actual decision made and its key aspects
- **Consequences**: The positive and negative outcomes of this decision
- **General Notes**: Personal reflections, implementation insights, and rationale useful for future blog posts or discussions

Note: We omit the "Status" field as our ADRs document decisions that have already been implemented.

## Index of ADRs

- [ADR-000: Technology Stack](./ADR-000-technology-stack.md)
- [ADR-001: Local-First Architecture](./ADR-001-local-first-architecture.md)
- [ADR-002: Zero-Knowledge Remote Vault Architecture](./ADR-002-zero-knowledge-remote-vault.md)
- [ADR-003: Vault Provider Abstraction Pattern](./ADR-003-vault-provider-abstraction.md)
- [ADR-004: QFX as Initial Import Format](./ADR-004-qfx-initial-import-format.md)
- [ADR-005: Source Generators for Controller Auto-Registration](./ADR-005-source-generators-controller-registration.md)
- [ADR-006: Copilot as Productivity Tool, Not Vibe Coding](./ADR-006-copilot-productivity-not-vibe-coding.md)
- [ADR-007: Protocol Buffers for Type-Safe Client-Server Communication](./ADR-007-protocol-buffers-communication.md)
- [ADR-008: Single UI Codebase Across All Platforms](./ADR-008-single-ui-codebase.md)
- [ADR-009: Runtime Platform Detection](./ADR-009-runtime-platform-detection.md)
- [ADR-010: Zylance File Formats](./ADR-010-zylance-file-formats.md)

## When to Create an ADR

Create an ADR when making a decision that:

- Affects the overall architecture or structure of the application
- Has significant long-term implications
- Involves trade-offs between multiple viable options
- Would benefit from documented context for future reference
- Changes or reverses a previous architectural decision

## How to Create a New ADR

1. Copy the template structure from an existing ADR
2. Number it sequentially (e.g., ADR-009)
3. Write a descriptive title that captures the essence of the decision
4. Fill in each section thoughtfully:
   - **Context**: What problem are you solving? What constraints exist?
   - **Decision**: What did you choose and why?
   - **Consequences**: What are the trade-offs?
   - **General Notes**: Include personal insights, implementation details, or lessons learned
5. Add it to the index above
6. Commit and link to it in relevant documentation

## Principles

- **Immutability**: Once written, ADRs should not be modified except for typos or clarity. If a decision changes, create a new ADR that supersedes the old one.
- **Brevity**: Keep ADRs focused and concise. Aim for clarity over completeness.
- **Honesty**: Document both positive and negative consequences. Include trade-offs and limitations.
- **Timeliness**: Write ADRs close to when the decision is made, while context is fresh.

## References

- [Michael Nygard's ADR Format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [ADR GitHub Organization](https://adr.github.io/)
- [Documenting Architecture Decisions](https://www.thoughtworks.com/radar/techniques/lightweight-architecture-decision-records)
