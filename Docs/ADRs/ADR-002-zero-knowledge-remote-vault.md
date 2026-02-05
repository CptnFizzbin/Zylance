# ADR-002: Zero-Knowledge Remote Vault Architecture

## Context

While Zylance v1 focuses on local-first architecture, users will eventually want to sync their financial data across multiple devices. This requires some form of remote storage or sync service. However, financial data is highly sensitive, and users should not have to trust the service provider (or even the developer) with access to their unencrypted data.

Traditional cloud sync approaches fall into two categories:
1. **Server-side storage**: Data is readable by the service provider (trust required)
2. **End-to-end encryption**: Data is encrypted client-side, but key management is complex

We needed to decide:
1. Should remote sync be centralized or distributed?
2. How do we ensure data privacy even from ourselves as developers?
3. How do we make the architecture flexible enough to support multiple sync backends?

## Implementation

**Status**: Planned

## Decision

Future versions of Zylance will support **zero-knowledge remote vault architecture**, where:

1. **All vault implementations conform to the same `IVaultProvider` interface**, making them swappable
2. **Remote vaults use zero-knowledge encryption**—data is encrypted client-side before transmission
3. **The developer cannot read user data**—encryption keys never leave the user's devices
4. **Remote vaults are self-hostable**—users can run their own sync servers
5. **Local and remote vaults are treated equally** through the abstraction layer

This means:
- Encryption happens in the client before data touches the network
- The sync server only stores encrypted blobs; it cannot decrypt them
- Users can choose between: local-only, self-hosted remote, or managed remote service
- Switching vault types is a configuration change, not a code rewrite
- All vault operations go through the same `IVaultProvider` interface

## Consequences

### Positive

- **Maximum privacy**: Even the developer cannot access user data
- **User choice**: Users can self-host or use managed services
- **Zero trust**: No need to trust the service provider
- **Portable architecture**: Easy to add new vault types (Dropbox, Google Drive, etc.)
- **Local-first compatible**: Remote sync is an enhancement, not a replacement
- **Open source friendly**: Community can audit the encryption implementation
- **Regulatory compliance**: Easier to comply with privacy laws (GDPR, etc.)

### Negative

- **Key management complexity**: Users must manage their encryption keys safely
- **Recovery challenges**: Lost keys = lost data (by design, but users may struggle)
- **Performance overhead**: Client-side encryption adds computational cost
- **Sync conflict resolution**: More complex when server can't read data to merge
- **Development complexity**: Encryption and key management add significant code complexity
- **Testing difficulty**: Need to test encryption, key rotation, and failure scenarios

### Mitigations

- Provide clear user education about key management and backup
- Implement key derivation from memorable passphrases (with strong KDF like Argon2)
- Consider multi-device key sharing protocols (like Signal's device linking)
- Add optional key backup mechanisms (encrypted with recovery phrase)
- Design sync conflict UI that works with encrypted data
- Thoroughly document the encryption architecture for security audits

## General Notes

This decision is inspired by zero-knowledge architectures like Signal, Bitwarden, and ProtonMail. The key insight is that the sync server should be a "dumb pipe" that stores encrypted blobs without any ability to decrypt them.

The `IVaultProvider` abstraction is critical here—it allows us to build the local vault first (v1) and add remote vaults later without refactoring the entire application. The interface hides whether data is stored locally or remotely, and whether it's encrypted or not.

One interesting challenge is sync conflict resolution. Traditional sync services can do smart merging by understanding the data structure. With zero-knowledge encryption, the server only sees encrypted blobs, so conflict resolution must happen client-side after decryption. This is solvable but requires careful design.

Another consideration is the trade-off between security and usability. We need to make encryption transparent to users during normal operation, but provide clear guidance about key backup and recovery. The recovery phrase approach (like crypto wallets use) seems promising.

Self-hosting is important for the open-source community and for users with strict privacy requirements. The sync server should be simple enough that anyone can run it on a Raspberry Pi or cheap VPS.

**For future blog post**: The zero-knowledge architecture is a fascinating case study in "security by design." Could explore the technical challenges of client-side encryption, key management UX, and the philosophical question of whether data you can't read is still your data. Also worth discussing the tension between "maximum security" and "grandmother can use it."
