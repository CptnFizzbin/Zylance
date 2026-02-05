# ADR-004: QFX as Initial Import Format

## Context

Users need to import financial data from their banks to populate Zylance with transactions. Banks provide data in various formats:
- **QFX/OFX**: Quicken Financial eXchange (XML-based, widely supported)
- **CSV**: Comma-separated values (simple but unstandardized)
- **OFX 2.x**: Newer XML version of OFX
- **FIX**: Financial Information eXchange
- **Proprietary formats**: Bank-specific formats

For v1, we needed to choose an initial import format that would:
1. Be supported by the developer's own bank (dogfooding)
2. Cover a significant number of users' banks
3. Be technically interesting to implement
4. Not have licensing or legal restrictions

We also needed to decide whether to use existing parsing libraries or write a custom parser.

## Implementation

**Status**: In Progress

## Decision

Implement **QFX (Quicken Financial eXchange)** as the first supported import format with a **custom parser**.

Rationale:
1. **Developer's bank supports it**: Enables dogfooding and real-world testing
2. **Wide adoption**: QFX/OFX is supported by most US banks and financial institutions
3. **Technically interesting**: The SGML-like format is challenging and educational
4. **Library limitations**: Existing NuGet libraries are either:
   - Unmaintained (last updated years ago)
   - Commercially restricted (require licenses for production use)
   - Incomplete (missing features we need)
5. **Learning opportunity**: Writing a parser teaches format internals

The parser implementation:
- Handles OFX 1.x format (SGML-like, not standard XML)
- Uses a two-stage parsing approach: raw tokenization → structured model
- Is built to be extensible for OFX 2.x (XML) support later
- Includes comprehensive tests with real-world bank files

Future formats will include:
- **CSV** (second priority): Universal but unstandardized
- **MS Money** (third priority): For users migrating from Microsoft Money

## Consequences

### Positive

- **No licensing issues**: Custom parser means no third-party license restrictions
- **Full control**: Can handle edge cases and bank-specific quirks
- **Learning value**: Deep understanding of financial data formats
- **Extensibility**: Easy to add support for bank-specific OFX variations
- **Testing**: Can write targeted tests for specific parsing scenarios
- **Maintenance**: No dependency on unmaintained libraries
- **Open source**: Parser can be reused by others facing the same library limitations

### Negative

- **Development time**: Writing a parser takes more time than using a library
- **Edge cases**: Must discover and handle format quirks ourselves
- **Standards complexity**: OFX spec is long and has many optional features
- **Maintenance burden**: We own the parser bugs and updates
- **Reinventing the wheel**: Duplicating work that exists (albeit imperfectly) elsewhere
- **Security**: Parser bugs could lead to crashes or vulnerabilities

### Mitigations

- Start with a minimal viable parser covering common cases
- Add comprehensive tests using real bank files (sanitized)
- Reference the official OFX specification for correctness
- Contribute parser back to community as separate library if it becomes robust
- Consider OFX libraries again in the future if they improve
- Focus on OFX 1.x first, defer OFX 2.x until needed
- Implement defensive parsing with clear error messages

## General Notes

The decision to write a custom parser was influenced by the frustrating state of .NET OFX libraries. The most popular ones are either abandoned or require expensive licenses. This is a common problem in financial software—formats are standardized but implementations are spotty.

The OFX 1.x format is particularly interesting because it's SGML-like but not quite XML. Tags can be self-closing or container-style, and the format is whitespace-sensitive in unusual ways. This makes standard XML parsers fail on OFX 1.x files.

Our two-stage parsing approach separates concerns:
1. **Raw parser**: Tokenizes the SGML-like syntax into elements and tokens
2. **Model parser**: Transforms tokens into strongly-typed C# models

This separation makes the code more maintainable and testable. The raw parser handles format quirks, while the model parser focuses on business logic.

The parser was a great opportunity to use modern C# features:
- Source-generated regex for performance
- Records for immutable data models  
- Pattern matching for token processing
- Extension methods for readable parsing code

**Real-world challenges encountered:**
- Banks don't always follow the spec (surprise!)
- Date formats vary (with/without timezone, with/without GMT offset)
- Some banks use non-standard field names
- Transaction types are inconsistent across institutions

One interesting aspect is that this parser could become a standalone library. If it proves robust, it might help others in the .NET ecosystem who face the same "no good OFX library" problem.

Future enhancement: Could add a "format detective" that auto-identifies file type, making imports more user-friendly. "Just drop your bank file here and we'll figure it out."

**For future blog post**: The journey of writing a financial data format parser would make a great technical blog post. Could cover: regex performance, parsing state machines, handling real-world format violations, and the decision process of "build vs. buy" for libraries. Also worth discussing the economics of financial data formats and why standardization doesn't guarantee good implementations.
