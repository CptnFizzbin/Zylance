# ADR-010: Zylance File Formats

## Context

Zylance needs to store user financial data locally in a format that is:
1. Secure (encrypted by default)
2. Portable (users can backup/restore easily)
3. Inspectable (users can see what data we're storing)
4. Standard (based on widely-supported technologies)
5. Migration-friendly (prevents vendor lock-in)

We needed to decide:
1. What file format to use for local storage?
2. Should encryption be mandatory or optional?
3. How do we balance security with transparency?
4. How do we prevent vendor lock-in?

## Implementation

**Status**: Planned

## Decision

Zylance will use **SQLite database** as the storage format with two variants:

### 1. Zylance Vault (.zlv) - Encrypted SQLite Database

**Default format** for production use:
- Encrypted SQLite database using SQLCipher or similar encryption
- File extension: `.zlv` (Zylance Vault)
- Encryption protects sensitive financial data at rest
- Requires passphrase/key to open
- Standard SQLite format underneath (once decrypted)

### 2. Zylance Database (.zld) - Plaintext SQLite Database

**Optional format** for specific use cases:
- Unencrypted SQLite database
- File extension: `.zld` (Zylance Database)
- Can be opened with any SQLite browser
- Useful for:
  - Development and debugging
  - Users who want to inspect stored data
  - Migration to other systems
  - Preventing vendor lock-in

### User Choice

An option in the UI allows users to toggle between encrypted and unencrypted formats. This gives users control over the security/transparency tradeoff.

**Rationale:**
- **Security by default**: Production users get encryption automatically
- **Transparency when needed**: Users can inspect what we're storing
- **Standard format**: SQLite is widely supported and future-proof
- **No vendor lock-in**: Users can access their data with standard tools
- **Migration-friendly**: Easy to export data to other formats

## Consequences

### Positive

- **Security by default**: Financial data is encrypted at rest
- **Standard technology**: SQLite is battle-tested and widely supported
- **User control**: Users can choose encryption vs. transparency
- **No vendor lock-in**: Users can access data with standard SQLite tools
- **Easy backups**: Simple file-based backups
- **Migration-friendly**: Standard format makes data migration easier
- **Inspection capability**: Developers and advanced users can inspect data
- **Cross-platform**: SQLite works on all platforms

### Negative

- **Encryption complexity**: Key management is complex
- **Two formats to maintain**: Need to support both encrypted and plaintext
- **User education**: Users need to understand the security implications of unencrypted databases
- **Recovery challenges**: Lost encryption keys mean lost data
- **Migration overhead**: Converting between formats requires careful handling
- **Format confusion**: Users might not understand the difference between .zlv and .zld

### Mitigations

- Provide clear UI explaining the difference between formats
- Make encryption the default with clear security warnings for plaintext
- Implement key derivation from memorable passphrases
- Add optional key backup mechanisms (encrypted with recovery phrase)
- Provide migration tools to convert between formats
- Document the format specifications for transparency
- Use strong encryption (AES-256 or equivalent)
- Consider file format versioning for future changes

## General Notes

The decision to support both encrypted and plaintext formats is deliberate. While security experts might argue for mandatory encryption, we believe users should have control over their data and understand what we're storing.

The plaintext option serves multiple purposes:
1. **Development**: Makes debugging and testing easier
2. **Transparency**: Users can verify what data we collect
3. **Trust building**: "Nothing to hide" builds user confidence
4. **Migration**: Users aren't locked into Zylance forever

**SQLite was chosen because:**
- It's the most deployed database engine in the world
- Zero configuration required
- Single file per database (easy backups)
- Cross-platform and stable
- Well-documented and widely understood
- Good performance for local-first apps
- Strong ecosystem of tools and libraries

**File extension choices:**
- `.zlv` suggests "vault" (secure, encrypted)
- `.zld` suggests "database" (standard, inspectable)
- Both clearly indicate Zylance format
- Different extensions prevent accidental opening with wrong tools

**Encryption approach:**
The encryption will likely use SQLCipher or a similar solution that provides transparent encryption at the SQLite layer. This means:
- Application code doesn't change between formats
- Just provide encryption key when opening encrypted databases
- Same SQLite API for both formats

**Key management considerations:**
- Derive keys from user passphrases using strong KDF (Argon2)
- Store key derivation parameters in database metadata
- Support optional key backup/recovery mechanisms
- Consider hardware security module (HSM) support for advanced users

**Future considerations:**
- Could add export to JSON for maximum portability
- Could support encrypted backups to cloud storage
- Could implement database compression for large datasets
- Could add format migration utilities

**For future blog post**: Could explore the tension between "security by default" and "user transparency." Most apps choose one or the other, but Zylance deliberately supports both. This reflects a philosophy that users should understand and control what software does with their data, even if it means they might choose less secure options.
