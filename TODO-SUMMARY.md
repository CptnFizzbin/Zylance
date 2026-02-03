# Zylance Project - TODO Items

This document catalogs all TODO, FIXME, HACK, XXX, and NOTE items found in the Zylance project codebase.

**Generated:** 2026-02-03

---

## Summary

- **Total TODOs:** 4
- **Total NOTEs:** Multiple (mostly in generated code)

---

## TODO Items

### 1. LocalVault - Lock State Management
**File:** `Zylance.Vault.Local/LocalVault.cs:25`  
**Priority:** Medium  
**Description:**
```csharp
public bool Locked => false; // TODO: Implement lock state management
```
**Context:** The vault currently reports as always unlocked. Lock state management needs to be implemented for proper vault security.

---

### 2. UseRuntime Hook - Runtime Detection
**File:** `Zylance.UI/Src/Hooks/UseRuntime.ts:8`  
**Priority:** Medium  
**Description:**
```typescript
// TODO: Implement actual runtime detection logic
```
**Context:** The runtime detection logic in the React hook needs to be properly implemented to detect the execution environment.

---

### 3. MenuRibbon Component - Placeholder Implementation
**File:** `Zylance.UI/Src/Components/MenuRibbon/MenuRibbon.tsx:8`  
**Priority:** High  
**Description:**
```typescript
* TODO: This is a PLACEHOLDER - Replace with actual ribbon menu implementation
```
**Context:** The MenuRibbon component is currently a placeholder and needs a full implementation.

---

### 4. AccountsPanel Component - Placeholder Implementation
**File:** `Zylance.UI/Src/Components/AccountsPanel/AccountsPanel.tsx:7`  
**Priority:** High  
**Description:**
```typescript
* TODO: This is a PLACEHOLDER - Implement with actual account data
```
**Context:** The AccountsPanel component is a placeholder and needs to be implemented with real account data integration.

---

### 5. AccountService - Vault Data Retrieval
**File:** `Zylance.Core/App/Services/AccountService.cs:23`  
**Priority:** Medium  
**Description:**
```csharp
// TODO: Implement actual retrieval from vault
```
**Context:** The AccountService needs implementation for retrieving account data from the vault provider.

---

## Generated Code TODOs

The following TODO items are in generated Protocol Buffer code (`Zylance.UI/Generated/google/protobuf/descriptor.ts`) and are part of the protobuf library itself, not project-specific:

1. Line 305: `TODO: flip the default to DECLARATION once all empty ranges`
2. Line 786: `TODO: clarify exactly what kinds of field types this option`
3. Line 1026: `TODO This is legacy behavior we plan to remove once downstream`
4. Line 1055: `TODO: make ctype actually deprecated.`
5. Line 1450: `TODO Remove this legacy behavior once downstream teams have`
6. Line 1634: `TODO Enums in C++ gencode (and potentially other languages) are`

**Note:** These items are from generated protobuf definitions and should not be modified directly in this project.

---

## Implementation Notes

### Important NOTEs in Code

1. **LocalVault.cs:36** - EF Core nested transactions support via savepoints in SQLite
2. **README.md:32** - Build process uses `NODE_HOME` for yarn location
3. **LocalVault README.md** - Contains general notes section about the local vault implementation
4. **AnalyzerReleases.Unshipped.md** - Standard template for analyzer release notes

---

## Recommendations

### High Priority
1. Implement MenuRibbon component with actual ribbon functionality
2. Implement AccountsPanel with real account data

### Medium Priority
1. Implement vault lock state management for security
2. Implement runtime detection logic in UseRuntime hook
3. Complete AccountService vault data retrieval

### Low Priority
- Review generated protobuf TODOs (though these are likely upstream concerns)

---

## Next Steps

When addressing these TODOs, prioritize based on:
1. User-facing features (UI components)
2. Security features (vault locking)
3. Data access features (account service)
4. Development/debugging features (runtime detection)

Each TODO should be addressed in a separate issue/PR with proper testing and documentation.
