# OFX File Format - Extractable Information

## Overview
OFX (Open Financial Exchange) is a standard format for electronic financial data exchange between financial institutions and their customers. This document outlines the types of information that can be extracted from OFX files.

## Currently Implemented

### OfxStatement
Each statement contains:
- **Account**: Bank account information
- **LedgerBalance**: Official account balance
- **AvailableBalance**: Available balance (optional)
- **Transactions**: List of transactions for the statement period

### 1. Bank Accounts (`OfxBankAccount`)
Located in `<BANKACCTFROM>` or `<BANKACCTTO>` elements:
- **BankId**: Bank routing number or identification
- **AccountId**: Account number
- **AccountType**: Type of account (CHECKING, SAVINGS, MONEYMRKT, CREDITLINE)
- **Currency**: Currency code (e.g., USD, CAD, EUR) - extracted from parent `STMTRS` element
- **Type**: Account type category (BANK, CREDITCARD, INVESTMENT, LOAN)

### 2. Transactions (`OfxTransaction`)
Located in `<STMTTRN>` elements within `<BANKTRANLIST>`:
- **Type**: Transaction type (DEBIT, CREDIT, INT, DIV, FEE, SRVCHG, DEP, ATM, POS, XFER, CHECK, PAYMENT, CASH, DIRECTDEP, DIRECTDEBIT, REPEATPMT, OTHER)
- **DatePosted**: Date and time the transaction was posted
- **Amount**: Transaction amount (negative for debits, positive for credits)
- **FitId**: Financial Institution Transaction ID (unique identifier)
- **Name**: Payee or description name
- **Memo**: Additional transaction notes/description
- **CheckNumber**: Check number (if applicable)
- **ReferenceNumber**: Reference number (if applicable)
- **IsTransfer**: Flag indicating if this is an inter-account transfer (Type = XFER)

### 3. Balances (`OfxBalance`)
Located in `<LEDGERBAL>` and `<AVAILBAL>` elements:
- **Amount**: Balance amount
- **AsOfDate**: Date/time the balance was accurate
- **Type**: Balance type (LEDGER or AVAIL)

## Additional Information Available in OFX Files

### 4. Sign-On Information
Located in `<SIGNONMSGSRSV1>/<SONRS>`:
- **Status**: Response status (code, severity, message)
- **ServerDateTime**: Server timestamp
- **Language**: Language code
- **Financial Institution Info**: Organization name and FID

### 5. Credit Card Accounts
Similar to bank accounts but in `<CREDITCARDMSGSRSV1>`:
- Account information in `<CCACCTFROM>` - Can be treated as `OfxBankAccount` with `Type` = "CREDITCARD"
- Credit card transactions in `<CCSTMTTRNRS>` - Same as `OfxTransaction`
- Credit limit information
- Reward/points balance (if available)

### 6. Investment Accounts
Located in `<INVSTMTMSGSRSV1>`:
- **Investment Account Info**: Account number, broker ID - Can be treated as `OfxBankAccount` with `Type` = "INVESTMENT"
- **Investment Transactions**: Buy, sell, dividend, interest, fees
- **Position Information**: Current holdings, cost basis, market value
- **Securities**: Stock symbols, CUSIP numbers, security names

### 7. Statement Period Information
Located in `<BANKTRANLIST>` or `<CCSTMTTRANLIST>` - Added to `OfxStatement`:
- **DateStart**: Beginning date of statement period - Available in `OfxStatement.DateStart`
- **DateEnd**: Ending date of statement period - Available in `OfxStatement.DateEnd`

### 8. Loan/Mortgage Information
Located in `<LOANMSGSRSV1>`:
- Principal balance - Can be treated as `OfxBankAccount` with `Type` = "LOAN"
- Interest rate
- Payment amount
- Next payment date

### 9. Transfer Information
For inter-account transfers:
- Source account
- Destination account
- Transfer amount
- Transfer date
- **Note**: The `IsTransfer` flag on `OfxTransaction` indicates when a transaction is a transfer (Type = XFER). The importer is responsible for matching up the transactions across accounts.

### 10. Bill Payment Information
Located in `<BILLPAYMSGSRSV1>` - **Future Feature**:
- Payee information
- Payment status
- Payment amount and date
- Confirmation numbers
- Recurrent payment schedules

## OFX Versions

- **OFX 1.x**: SGML-based format (currently implemented in `Zylance.Core.Lib.Importers.Ofx.V1`)
- **OFX 2.x**: XML-based format with schema validation (to be added later)
- **QFX**: Quicken proprietary extension of OFX 1.x (handled identically to OFX 1.x)

## Parser Architecture

- **Top-level Parser**: `OfxParser` - Detects OFX version and delegates to appropriate version parser
- **V1 Parser**: `OfxV1Parser` - Parses OFX 1.x/SGML format
- **Common Models**: `OfxStatement`, `OfxBankAccount`, `OfxTransaction`, `OfxBalance` - Shared across all versions

## Future Enhancement Opportunities

Additional data types that could be extracted:
1. **OfxCreditCardStatement** - Credit card specific statement information with credit limits
2. **OfxInvestmentStatement** - Brokerage/investment account statement with positions
3. **OfxInvestmentTransaction** - Stock trades, dividends, etc.
4. **OfxPosition** - Current investment holdings
5. **OfxSecurity** - Security definitions (stocks, bonds, funds)
6. **OfxPayee** - Bill payment payee information
7. **OfxTransferDetail** - Detailed transfer information with source/destination accounts
8. **OfxStatementInfo** - Statement period and metadata
9. **OfxStatus** - Status codes and messages
10. **OfxInstitution** - Financial institution details

## References

The OFX specification and documentation can be found through financial institution developer resources and banking technology standards organizations.
