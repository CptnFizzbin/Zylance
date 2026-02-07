# Import/Export Formats

This directory contains documentation and specifications for the various file formats that Zylance supports for importing and exporting financial data.

## Supported Formats

### QFX/OFX (Quicken Financial eXchange)

**Status**: ✅ Implemented (v1)  
**Priority**: Primary import format  
**Specification**: [OFX Specification Files](./Ofx/)

QFX (Quicken Financial eXchange) and OFX (Open Financial Exchange) are XML-based formats for exchanging financial data between institutions and personal finance software. QFX is Quicken's proprietary variant of the open OFX standard.

**Key Features:**
- **Wide bank support**: Most US financial institutions support OFX/QFX exports
- **Comprehensive data**: Includes transactions, balances, account info
- **Standardized**: Based on the OFX specification (SGML-like format for v1.x)
- **Transaction types**: Supports various transaction types (debit, credit, ATM, etc.)

**Format Details:**
- **Version 1.x**: SGML-like format (not standard XML)
- **Version 2.x**: XML format (future support planned)
- **Extensions**: .qfx, .ofx

**Implementation:**
Zylance includes a custom OFX 1.x parser built in C# that handles the SGML-like format. The parser uses a two-stage approach:
1. **Raw parsing**: Tokenizes the SGML-like syntax
2. **Model parsing**: Transforms tokens into strongly-typed models

**See Also:**
- [ADR-004: QFX as Initial Import Format](../ADRs/ADR-004-qfx-initial-import-format.md) - Decision rationale
- [OFX Specification Documents](./Ofx/) - Official OFX specs and DTDs

### CSV (Comma-Separated Values)

**Status**: 🔄 Planned (future)  
**Priority**: Second priority import format

CSV is a simple, universal format supported by virtually all financial institutions and spreadsheet applications. However, CSV lacks standardization—each bank uses its own column structure, date formats, and conventions.

**Challenges:**
- **No standard schema**: Each institution has different column layouts
- **Date format variations**: No consistent date representation
- **Encoding issues**: Character encoding varies by institution
- **Ambiguous data**: Transaction types, categories often encoded differently

**Planned Implementation:**
- Bank-specific CSV parsers (detect bank by file structure)
- User-configurable mapping for custom formats
- Intelligent field detection and validation
- Date format auto-detection

### Microsoft Money (OFC/OFX)

**Status**: 📋 Planned (future)  
**Priority**: Third priority import format

Microsoft Money used variants of OFX and a proprietary OFC format. Support for these formats will help users migrate from the discontinued Microsoft Money application.

**Notes:**
- Microsoft Money was discontinued in 2009
- Files may use .mny (proprietary binary) or .ofx/.ofc (XML-based)
- Focus will be on the XML-based formats initially

## Future Format Support

Potential formats for future consideration:

- **IIF (Intuit Interchange Format)**: QuickBooks format
- **YNAB (You Need A Budget)**: CSV variant with specific structure
- **Mint**: JSON export format
- **Zylance Vault (.zlv)**: Encrypted SQLite database (see [ADR-010: Zylance File Formats](../ADRs/ADR-010-zylance-vault-file-formats.md))
- **Zylance Database (.zlv.sqlite)**: Plaintext SQLite database (see [ADR-010: Zylance File Formats](../ADRs/ADR-010-zylance-vault-file-formats.md))
- **Excel**: XLSX import with configurable mapping

## Format Guidelines

When adding support for a new format:

1. **Create a parser**: Implement `ITransactionImporter` interface
2. **Add tests**: Include real-world sample files (sanitized/anonymized)
3. **Document structure**: Add format documentation to this directory
4. **Error handling**: Provide clear error messages for invalid files
5. **Validation**: Validate data integrity and ranges
6. **Consider edge cases**: Handle bank-specific quirks and variations

## Testing with Real Data

**Important**: When testing with real financial data:
1. **Sanitize/anonymize files**: Remove account numbers, personal info, real amounts
2. **Preserve structure**: Keep format structure intact for testing
3. **Add to test fixtures**: Include sanitized files in test suite
4. **Document source**: Note which institution/bank the format comes from

## Contributing Format Support

To contribute support for a new format:

1. Check if format is widely used and documented
2. Obtain sample files (sanitized) from real institutions
3. Implement parser following existing patterns
4. Add comprehensive tests
5. Document the format in this directory
6. Update this README with format details
7. Consider creating an ADR if the decision involves trade-offs

## Format Specifications

This directory contains official specifications and documentation for supported formats:

- **Ofx/**: OFX/QFX specification documents and DTDs (Document Type Definitions)

## See Also

- [ADR-004: QFX as Initial Import Format](../ADRs/ADR-004-qfx-initial-import-format.md) - Why QFX was chosen first
- [Zylance.Core/Importers](../../Zylance.Core/Importers/) - Parser implementations
- [Test Fixtures](../../Zylance.Core.Tests/Fixtures/Importers/) - Sample files for testing
