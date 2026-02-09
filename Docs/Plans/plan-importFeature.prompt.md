# Plan: Import Feature Implementation

The Import feature enables users to import financial transactions from external
files (initially QFX/OFX format). The flow involves file selection, async
parsing/importing, validation of new accounts, and progress reporting to the UI
via background task events.

## Communication Flow

```
# user clicks on import button
# user selects a file to import
ui -> core: import:startReq(fileRef)
ui <- core: import:startRes(importId)
ui <- core: import:importStartedEvt(importId)
# core reads file and parses it
ui <- core: import:importProgressEvt(importId, progress) [repeated]
if file contains new accounts:
  ui <- core: import:getAccountInfoEvt(importId, accounts[])
  # user fills in account info
  ui -> core: import:setAccountInfoEvt(importId, accounts[])
  # if there are errors, repeat the above two steps until all accounts are valid
  # core creates accounts
# core imports legerEntries
ui <- core: import:importProgressEvt(importId, progress) [repeated]
ui <- core: import:completeEvt(results(numTransactions))
```

## Implementation Steps

### 1. Define Import Proto Messages

**File**: `Zylance.Contract/zylance/api/Import.proto`

Create request/response actions:

- `Import_StartRes` - contains `FileRef`
- `Import_StartReq` - returns `importId` (string UUID)

Create events:

- `Import_ProgressEvt` - contains `importId`, `progress` (0-1 float), optional
  `description`
- `Import_GetAccountsEvt` - sent by core to request account info, contains
  `importId` and list of `PartialAccountData` (accounts needing validation)
- `Import_SetAccountsEvt` - sent by UI with validated account data, contains
  `importId` and list of `AccountData`
- `Import_CompleteEvt` - final event with `importId` and `ImportResult`

Create models:

- `PartialAccountData` - account details requiring user input (name, type, bank
  name, account number)
- `AccountData` - filled details for account creation
- `ImportResult` - contains `numTransactions` (int), `numAccounts` (int),
  optional errors

Future improvement:
replace Import_GetAccountsEvt & Import_SetAccountsEvt with a request-response
pair to match the current pattern. Events are typically for one-way
notifications, while requests expect a reply, but events can be listened for
and requests are typically handled by a controller.

### 2. Create ImportService

**File**: `Zylance.Core/App/Services/ImportService.cs`

Responsibilities:

- Orchestrate the import workflow using `IFileProvider` to read files
- Delegate to format-specific importers (`OfxImporter`)
- Track import sessions by `importId` (in-memory map or state object)
- Manage account validation loop: detect new accounts → emit
  `ImportAccountsReqEvt` → await `ImportAccountsSetEvt` → validate → repeat if
  errors → create accounts
- Report progress via `BackgroundTaskService.WithProgress()` callback
- Emit all events via Gateway

Key methods:

```cs
class ImportService
{
  // initiates import, returns importId, and starts background work
  public ImportSession StartImport(FileRef fileRef) 
  {
    // generate importId
    // create ImportSession and store in dictionary
    // start background task to execute import logic
    // registers ImportSession for disposal on Vault:Closed event
    // return importId in response
  }
}

class ImportSession(Zylance zylance)
{
  public async Task Run() 
  {
    // read file content using IFileProvider
    // parse file with OfxImporter, report progress
    //
    // while no errors
    //     emit ImportGetAccountsEvt for all accounts
    //     wait for ImportSetAccountsEvt with account info
    //     validate accounts
    //
    // using VaultScope
    //     for each account
    //         upsert account in vault
    //         report progress
    //     
    //     for each transaction
    //         create ledger entry
    //         report progress
    //
    // emit ImportCompleteEvt with results
  }
}
```

### 3. Implement ImportController

**File**: `Zylance.Core/App/Controllers/ImportController.cs`

Add handler:

```cs
[RequestHandler] 
public Task StartImport(ZyRequest<ImportStartReq> req, ZyResponse<ImportStartRes> res) 
{
    // calls `ImportService.StartImport(fileRef)`
    // returns ImportSession.Id in response

    // Improvement: Allow for ZyResponse to be sent early before the controller returns
}
```

The controller delegates business logic to the service while handling
request/response marshaling.

### 4. Extend Proto Models

**File**: `Zylance.Contract/zylance/models/Import.proto`

Define `PartialAccountData` with fields:

- `account_number` (string) - from parsed file
- `bank_name` (string) - from parsed file
- `account_type` (string) - suggested type from parser
- `balance` (double) - current balance from file

Define `AccountData` with fields:

- `account_number` (string)
- `bank_name` (string)
- `name` (string, required) - user-provided display name
- `account_type` (string, required) - user-confirmed type (Checking, Savings,
  CreditCard, Investment, Loan)
- `balance` (double)

Add validation error messages to proto:

- Define `AccountValidationError` with fields: `account_number`, `field_name`,
  `error_message`

### 5. Create LedgerService

**File**: `Zylance.Core/App/Services/LedgerService.cs`

Create a new service to handle ledger entry operations:

Key responsibilities:

- Create ledger entries in the vault
- Check for duplicate transactions using composite key (date + amount +
  description + account)
- Batch insert transactions for performance
- Return summary of created vs. skipped (duplicate) entries

Key methods:

```cs
class LedgerService
{
    // Returns count of created entries (excluding duplicates)
    public Task<int> CreateLedgerEntriesAsync(
        List<LedgerEntryData> entries, 
        CancellationToken cancellationToken
    );
    
    // Check if transaction already exists
    private Task<bool> IsDuplicateTransactionAsync(
        string accountId,
        DateTimeOffset date,
        decimal amount,
        string description,
        CancellationToken cancellationToken
    );
}
```

Register `LedgerService` in `Zylance.cs` DI setup.

### 6. Add Import Error Event

**File**: `Zylance.Contract/zylance/api/Import.proto`

Add error event:

- `Import_ErrorEvt` - contains `importId`, `error_code` (string),
  `error_message` (string)

Error codes to support:

- `FILE_READ_ERROR` - cannot read file
- `PARSE_ERROR` - file format invalid
- `ACCOUNT_VALIDATION_ERROR` - user-provided account info invalid
- `IMPORT_TIMEOUT` - session abandoned after timeout
- `UNKNOWN_ERROR` - unexpected error

Update `ImportService.Run()` to emit `Import_ErrorEvt` on exceptions.

### 7. Update UI Bindings

**File**: `Zylance.UI/Lib/`

- Generate TypeScript types from proto
- Create `useImport()` hook in `Zylance.UI/Src/Hooks/` or `Zylance.UI/Lib/` for:
  - Sending `StartImport` request
  - Subscribing to `Import:Progress`, `Import:GetAccounts`, `Import:Complete`,
    and `Import:Error` events
  - emitting `Import:SetAccounts` event in response to account request
- Create React component(s) for import workflow (file selection, progress
  display, account info form)

Implement stepper-style progress indicator:

```
( ) ========= ( ) ====>---- ( ) --------- ( )
 File Read     Accounts      Import        Complete
```

Add client-side validation for account info form:

- Name: required, non-empty
- Account type: required, must be one of enum values
- Format validation for account numbers (if applicable)

### 8. Implement Account Validation in ImportService

**File**: `Zylance.Core/App/Services/ImportService.cs`

Add validation logic in `ImportSession.Run()`:

```cs
private List<AccountValidationError> ValidateAccounts(List<AccountData> accounts)
{
    var errors = new List<AccountValidationError>();
    
    // Check for duplicate names within submission
    var duplicateNames = accounts
        .GroupBy(a => a.Name)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);
    
    foreach (var name in duplicateNames)
    {
        errors.Add(new AccountValidationError 
        { 
            AccountNumber = accounts.First(a => a.Name == name).AccountNumber,
            FieldName = "name",
            ErrorMessage = $"Account name '{name}' is used multiple times"
        });
    }
    
    // Check against existing vault accounts
    foreach (var account in accounts)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            errors.Add(new AccountValidationError 
            { 
                AccountNumber = account.AccountNumber,
                FieldName = "name",
                ErrorMessage = "Account name is required"
            });
        }
        
        if (vaultContext.AccountExists(account.Name, account.AccountType))
        {
            errors.Add(new AccountValidationError 
            { 
                AccountNumber = account.AccountNumber,
                FieldName = "name",
                ErrorMessage = $"Account '{account.Name}' already exists"
            });
        }
    }
    
    return errors;
}
```

### 9. Add Integration Tests

**File**: `Zylance.Core.Tests/App/Services/ImportServiceTests.cs`

Test scenarios:

**Happy path:**

- Import file with no new accounts (all accounts exist in vault)
- Verify correct number of transactions created
- Verify duplicate transactions are skipped

**New accounts flow:**

- Import file with new accounts
- Receive `Import:GetAccounts` event with `PartialAccountData`
- Send `Import:SetAccounts` event with complete `AccountData`
- Verify accounts are created in vault
- Verify transactions are linked to new accounts

**Account validation:**

- Send invalid account info (missing name)
- Receive `Import:GetAccounts` event again with validation errors
- Send corrected account info
- Verify import completes successfully

**Duplicate account names:**

- Send account info with duplicate names within submission
- Verify validation errors returned
- Send account info with name that already exists in vault
- Verify validation error returned

**Error handling:**

- Invalid file (FILE_READ_ERROR)
- Unsupported format (PARSE_ERROR)
- Parser failure on malformed content (PARSE_ERROR)
- File not found (FILE_READ_ERROR)
- Verify `Import:Error` event emitted with correct error code

**Concurrency:**

- Attempt to start second import while first is in progress
- Verify error response (single import at a time)

**Session timeout:**

- Start import, wait for account request
- Do not send account info for 5+ minutes
- Verify `Import:Error` event with IMPORT_TIMEOUT code
- Verify session cleaned up

**Cancellation:**

- Start import
- Cancel via CancellationToken
- Verify graceful shutdown
- Verify session cleaned up

**Progress reporting:**

- Mock progress callbacks during file read, account creation, transaction import
- Verify `Import:Progress` events emitted at each stage
- Verify progress values are clamped to [0.0, 1.0]

Use fixtures with sample OFX files from
`Zylance.Core.Tests/Fixtures/Importers/`.

## Dependencies

- `IFileProvider` - read file content
- `OfxImporter` (existing) - parse OFX/QFX files
- `BackgroundTaskService` - report progress to UI
- `GatewayService` - emit events
- `VaultContext` - access vault/accounts/ledger
- `LedgerService` (new) - create ledger entries with duplicate detection
- `CancellationToken` - support graceful cancellation

## Architecture Decisions

### Account Validation Strategy

**Decision**: Split validation between Core and UI

- **Core**: Validates business rules (no duplicate account names, vault
  consistency)
- **UI**: Provides immediate format validation (required fields, string lengths)
- **Rationale**: Better UX with immediate feedback while maintaining data
  integrity

### Progress Reporting

**Decision**: Multi-stage progress with stepper UI

- **Stages**: File Read (0-25%) → Account Validation (25-50%) → Import (
  50-100%) → Complete
- **UI Pattern**: Stepper-style progress indicator showing current stage
- **Rationale**: Clear visual feedback of import workflow state

### Concurrent Imports

**Decision**: Single import at a time

- **Enforcement**: Check active session before starting new import
- **Rationale**: Reduces complexity, prevents race conditions on account
  creation
- **Future**: Could support queue-based imports if needed

### Session Management

**Decision**: In-memory session storage with timeout

- **Storage**: `Dictionary<string, ImportSession>` in `ImportService`
- **Timeout**: 5 minutes of inactivity triggers cleanup and error event
- **Cleanup**: Remove on completion, error, or `Vault:Closed` event
- **Rationale**: Simple, stateless restarts, no persistence needed for
  short-lived operations

### Error Handling

**Decision**: Structured error events with error codes

- **Event**: `Import:Error` with `error_code` and `error_message`
- **Codes**: `FILE_READ_ERROR`, `PARSE_ERROR`, `ACCOUNT_VALIDATION_ERROR`,
  `IMPORT_TIMEOUT`, `UNKNOWN_ERROR`
- **Rationale**: UI can display user-friendly messages and handle errors
  appropriately

### Duplicate Detection

**Decision**: Composite key for transaction deduplication

- **Key**: `(accountId, date, amount, description)`
- **Service**: `LedgerService` handles duplicate checking
- **Behavior**: Skip duplicates, report count in completion event
- **Rationale**: Prevents accidental re-imports of same file

## Files to Create/Modify

| File                                                    | Action                          | Priority |
|---------------------------------------------------------|---------------------------------|----------|
| `Zylance.Contract/zylance/api/Import.proto`             | Create/Extend                   | High     |
| `Zylance.Contract/zylance/models/Import.proto`          | Create                          | High     |
| `Zylance.Core/App/Services/ImportService.cs`            | Create                          | High     |
| `Zylance.Core/App/Services/LedgerService.cs`            | Create                          | High     |
| `Zylance.Core/App/Controllers/ImportController.cs`      | Implement                       | High     |
| `Zylance.Core/Zylance.cs`                               | Modify (register LedgerService) | High     |
| `Zylance.Core.Tests/App/Services/ImportServiceTests.cs` | Create                          | High     |
| `Zylance.Core.Tests/App/Services/LedgerServiceTests.cs` | Create                          | Medium   |
| `Zylance.UI/Lib/useImport.ts`                           | Create                          | Medium   |
| `Zylance.UI/Src/Components/Import/`                     | Create                          | Medium   |
| `Zylance.Core.Tests/Fixtures/Importers/`                | Add test OFX files              | Medium   |

## Success Criteria

- ✅ User can select a file and initiate import via UI
- ✅ Core reports progress as file is parsed and transactions are imported
- ✅ If new accounts are detected, core prompts UI for account info
- ✅ User can fill in account details and retry validation
- ✅ Import completes successfully with transaction count in final event
- ✅ All major error cases are handled gracefully
- ✅ Integration tests cover happy path and account validation flow

