# OFX File Format - Extractable Information

## Overview
OFX (Open Financial Exchange) is a standard format for electronic financial data exchange between financial institutions and their customers. This document outlines the types of information that can be extracted from OFX files.

## Currently Implemented

### 1. Bank Accounts (`OfxBankAccount`)
Located in `<BANKACCTFROM>` or `<BANKACCTTO>` elements:
- **BankId**: Bank routing number or identification
- **AccountId**: Account number
- **AccountType**: Type of account (CHECKING, SAVINGS, MONEYMRKT, CREDITLINE)
- **Currency**: Currency code (e.g., USD, EUR) - extracted from parent `STMTRS` element

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
- Account information in `<CCACCTFROM>`
- Credit card transactions in `<CCSTMTTRNRS>`
- Credit limit information
- Reward/points balance (if available)

### 6. Investment Accounts
Located in `<INVSTMTMSGSRSV1>`:
- **Investment Account Info**: Account number, broker ID
- **Investment Transactions**: Buy, sell, dividend, interest, fees
- **Position Information**: Current holdings, cost basis, market value
- **Securities**: Stock symbols, CUSIP numbers, security names

### 7. Statement Period Information
Located in `<BANKTRANLIST>` or `<CCSTMTTRANLIST>`:
- **DateStart**: Beginning date of statement period
- **DateEnd**: Ending date of statement period

### 8. Loan/Mortgage Information
Located in `<LOANMSGSRSV1>`:
- Principal balance
- Interest rate
- Payment amount
- Next payment date

### 9. Transfer Information
For inter-account transfers:
- Source account
- Destination account
- Transfer amount
- Transfer date

### 10. Bill Payment Information
Located in `<BILLPAYMSGSRSV1>`:
- Payee information
- Payment status
- Payment amount and date
- Confirmation numbers

### 11. Tax Information
Some institutions include:
- Year-to-date interest earned
- Tax ID numbers
- 1099 information

## Future Enhancement Opportunities

To fully support all OFX capabilities, consider adding models for:
1. **OfxCreditCardAccount** - Credit card specific account information
2. **OfxInvestmentAccount** - Brokerage/investment account details
3. **OfxInvestmentTransaction** - Stock trades, dividends, etc.
4. **OfxPosition** - Current investment holdings
5. **OfxSecurity** - Security definitions (stocks, bonds, funds)
6. **OfxPayee** - Bill payment payee information
7. **OfxTransfer** - Account transfer details
8. **OfxStatementInfo** - Statement period and metadata
9. **OfxStatus** - Status codes and messages
10. **OfxInstitution** - Financial institution details

## OFX Versions

- **OFX 1.x**: SGML-based format (what we currently parse)
- **OFX 2.x**: XML-based format with schema validation
- **QFX**: Quicken proprietary extension of OFX 1.x (handled identically)

## References

- OFX Specification: https://www.ofx.net/
- Common Transaction Types: https://www.ofx.net/downloads/OFX%202.2.pdf
