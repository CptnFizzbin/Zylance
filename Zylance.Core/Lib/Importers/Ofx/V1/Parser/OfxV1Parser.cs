using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Parser;

/// <summary>
/// Parses OFX V1 (SGML) format files and returns structured statement data.
/// </summary>
public class OfxV1Parser
{
    private static class TagNames
    {
        public const string StatementTransactionResponse = "STMTTRNRS";
        public const string CreditCardStatementTransactionResponse = "CCSTMTTRNRS";
        public const string StatementResponse = "STMTRS";
        public const string CreditCardStatementResponse = "CCSTMTRS";
        public const string BankAccountFrom = "BANKACCTFROM";
        public const string CreditCardAccountFrom = "CCACCTFROM";
        public const string LedgerBalance = "LEDGERBAL";
        public const string AvailableBalance = "AVAILBAL";
        public const string BankTransactionList = "BANKTRANLIST";
        public const string StatementTransaction = "STMTTRN";
        public const string CurrencyDefinition = "CURDEF";
        public const string BankId = "BANKID";
        public const string AccountId = "ACCTID";
        public const string AccountType = "ACCTTYPE";
        public const string BalanceAmount = "BALAMT";
        public const string DateAsOf = "DTASOF";
        public const string DateStart = "DTSTART";
        public const string DateEnd = "DTEND";
        public const string TransactionType = "TRNTYPE";
        public const string DatePosted = "DTPOSTED";
        public const string TransactionAmount = "TRNAMT";
        public const string FitId = "FITID";
        public const string Name = "NAME";
        public const string Memo = "MEMO";
        public const string CheckNumber = "CHECKNUM";
        public const string ReferenceNumber = "REFNUM";
    }

    /// <summary>
    /// Parses an OFX V1 file and returns a list of statements.
    /// Each statement contains an account, balance information, and transactions.
    /// </summary>
    /// <param name="content">StreamReader containing the OFX file content</param>
    /// <returns>List of OFX statements parsed from the file</returns>
    public Task<List<OfxStatement>> ParseAsync(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        var statements = ExtractStatements(rawFile.Root);
        return Task.FromResult(statements);
    }

    private List<OfxStatement> ExtractStatements(OfxRawElement element)
    {
        var statements = new List<OfxStatement>();

        if (element.Name == TagNames.StatementTransactionResponse)
        {
            var statement = BuildStatement(element);
            if (statement is not null)
            {
                statements.Add(statement);
            }
        }
        else if (element.Name == TagNames.CreditCardStatementTransactionResponse)
        {
            var statement = BuildCreditCardStatement(element);
            if (statement is not null)
            {
                statements.Add(statement);
            }
        }

        foreach (var child in element.Children)
        {
            statements.AddRange(ExtractStatements(child));
        }

        return statements;
    }

    private OfxStatement? BuildStatement(OfxRawElement statementTransactionResponse)
    {
        var statementResponseElement = statementTransactionResponse.Children.FirstOrDefault(c =>
            c.Name == TagNames.StatementResponse
        );
        if (statementResponseElement is null)
            return null;

        var currency = GetCurrency(statementResponseElement);

        var bankAccountFromElement = statementResponseElement.Children.FirstOrDefault(c =>
            c.Name == TagNames.BankAccountFrom
        );
        if (bankAccountFromElement is null)
            return null;

        var account = BuildBankAccount(bankAccountFromElement, currency, "BANK");

        var (ledgerBalance, availableBalance) = GetBalances(statementResponseElement);

        var (transactions, dateStart, dateEnd) = GetTransactions(
            statementResponseElement,
            TagNames.BankTransactionList
        );

        return new OfxStatement
        {
            Account = account,
            LedgerBalance = ledgerBalance,
            AvailableBalance = availableBalance,
            Transactions = transactions,
            DateStart = dateStart,
            DateEnd = dateEnd,
        };
    }

    private OfxStatement? BuildCreditCardStatement(OfxRawElement creditCardStatementTransactionResponse)
    {
        var creditCardStatementResponseElement = creditCardStatementTransactionResponse.Children.FirstOrDefault(c =>
            c.Name == TagNames.CreditCardStatementResponse
        );
        if (creditCardStatementResponseElement is null)
            return null;

        var currency = GetCurrency(creditCardStatementResponseElement);

        var creditCardAccountFromElement = creditCardStatementResponseElement.Children.FirstOrDefault(c =>
            c.Name == TagNames.CreditCardAccountFrom
        );
        if (creditCardAccountFromElement is null)
            return null;

        var account = BuildBankAccount(creditCardAccountFromElement, currency, "CREDITCARD");

        var (ledgerBalance, availableBalance) = GetBalances(creditCardStatementResponseElement);

        var (transactions, dateStart, dateEnd) = GetTransactions(
            creditCardStatementResponseElement,
            TagNames.BankTransactionList
        );

        return new OfxStatement
        {
            Account = account,
            LedgerBalance = ledgerBalance,
            AvailableBalance = availableBalance,
            Transactions = transactions,
            DateStart = dateStart,
            DateEnd = dateEnd,
        };
    }

    private string? GetCurrency(OfxRawElement element)
    {
        return element.Tokens.TryGetValue(TagNames.CurrencyDefinition, out var currencyToken)
            ? currencyToken.Value
            : null;
    }

    private (OfxBalance ledger, OfxBalance? available) GetBalances(OfxRawElement element)
    {
        var ledgerBalanceElement = element.Children.FirstOrDefault(c => c.Name == TagNames.LedgerBalance);
        if (ledgerBalanceElement is null)
            throw new InvalidDataException("Missing LEDGERBAL element");

        var ledgerBalance = BuildBalance(ledgerBalanceElement, "LEDGER");

        var availableBalanceElement = element.Children.FirstOrDefault(c => c.Name == TagNames.AvailableBalance);
        var availableBalance = availableBalanceElement is not null
            ? BuildBalance(availableBalanceElement, "AVAIL")
            : null;

        return (ledgerBalance, availableBalance);
    }

    private (List<OfxTransaction> transactions, DateTimeOffset? start, DateTimeOffset? end) GetTransactions(
        OfxRawElement element,
        string transactionListTagName
    )
    {
        var transactions = new List<OfxTransaction>();
        DateTimeOffset? dateStart = null;
        DateTimeOffset? dateEnd = null;

        var bankTransactionListElement = element.Children.FirstOrDefault(c => c.Name == transactionListTagName);
        if (bankTransactionListElement is not null)
        {
            transactions = bankTransactionListElement
                .Children.Where(c => c.Name == TagNames.StatementTransaction)
                .Select(BuildTransaction)
                .ToList();

            if (
                bankTransactionListElement.Tokens.TryGetValue(TagNames.DateStart, out var dateStartToken)
                && DateTimeOffsetParser.TryParse(dateStartToken.Value, out var start)
            )
            {
                dateStart = start;
            }

            if (
                bankTransactionListElement.Tokens.TryGetValue(TagNames.DateEnd, out var dateEndToken)
                && DateTimeOffsetParser.TryParse(dateEndToken.Value, out var end)
            )
            {
                dateEnd = end;
            }
        }

        return (transactions, dateStart, dateEnd);
    }

    private OfxBankAccount BuildBankAccount(OfxRawElement element, string? currency, string accountType)
    {
        var accountId = element.Tokens.TryGetValue(TagNames.AccountId, out var accountIdToken)
            ? accountIdToken.Value
            : throw new InvalidDataException("Missing ACCTID in account element");

        // BANKID is only present in bank accounts (BANKACCTFROM), not credit card accounts (CCACCTFROM)
        var bankId = element.Tokens.TryGetValue(TagNames.BankId, out var bankIdToken) 
            ? bankIdToken.Value 
            : null;

        // ACCTTYPE is only present in bank accounts (BANKACCTFROM), not credit card accounts (CCACCTFROM)
        var accountTypeValue = element.Tokens.TryGetValue(TagNames.AccountType, out var accountTypeToken)
            ? accountTypeToken.Value
            : null;

        return new OfxBankAccount
        {
            BankId = bankId ?? string.Empty,
            AccountId = accountId,
            AccountType = accountTypeValue ?? string.Empty,
            Currency = currency,
            Type = accountType,
        };
    }

    private OfxTransaction BuildTransaction(OfxRawElement element)
    {
        var type = element.Tokens.TryGetValue(TagNames.TransactionType, out var typeToken)
            ? typeToken.Value
            : throw new InvalidDataException("Missing TRNTYPE in STMTTRN");

        var datePosted =
            element.Tokens.TryGetValue(TagNames.DatePosted, out var datePostedToken)
            && datePostedToken.DateTimeValue.HasValue
                ? datePostedToken.DateTimeValue.Value
                : throw new InvalidDataException("Missing or invalid DTPOSTED in STMTTRN");

        var amount =
            element.Tokens.TryGetValue(TagNames.TransactionAmount, out var amountToken)
            && amountToken.DecimalValue.HasValue
                ? amountToken.DecimalValue.Value
                : throw new InvalidDataException("Missing or invalid TRNAMT in STMTTRN");

        var fitId = element.Tokens.TryGetValue(TagNames.FitId, out var fitIdToken)
            ? fitIdToken.Value
            : throw new InvalidDataException("Missing FITID in STMTTRN");

        var isTransfer = type.Equals("XFER", StringComparison.OrdinalIgnoreCase);

        return new OfxTransaction
        {
            Type = type,
            DatePosted = datePosted,
            Amount = amount,
            FitId = fitId,
            Name = element.Tokens.TryGetValue(TagNames.Name, out var nameToken) ? nameToken.Value : null,
            Memo = element.Tokens.TryGetValue(TagNames.Memo, out var memoToken) ? memoToken.Value : null,
            CheckNumber = element.Tokens.TryGetValue(TagNames.CheckNumber, out var checkToken)
                ? checkToken.Value
                : null,
            ReferenceNumber = element.Tokens.TryGetValue(TagNames.ReferenceNumber, out var refToken)
                ? refToken.Value
                : null,
            IsTransfer = isTransfer,
        };
    }

    private OfxBalance BuildBalance(OfxRawElement element, string balanceType)
    {
        var amount =
            element.Tokens.TryGetValue(TagNames.BalanceAmount, out var amountToken) && amountToken.DecimalValue.HasValue
                ? amountToken.DecimalValue.Value
                : throw new InvalidDataException($"Missing or invalid BALAMT in {balanceType}BAL");

        var asOfDate =
            element.Tokens.TryGetValue(TagNames.DateAsOf, out var dateAsOfToken) && dateAsOfToken.DateTimeValue.HasValue
                ? dateAsOfToken.DateTimeValue.Value
                : throw new InvalidDataException($"Missing or invalid DTASOF in {balanceType}BAL");

        return new OfxBalance
        {
            Amount = amount,
            AsOfDate = asOfDate,
            Type = balanceType,
        };
    }
}
