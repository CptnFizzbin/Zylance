namespace Zylance.Core.Importers.Ofx.V1.Raw;

/// <summary>
/// OFX tag names used when parsing OFX v1 responses.
/// </summary>
public static class OfxTagNames
{
    /// <summary>Account identifier tag (ACCTID).</summary>
    public const string AccountId = "ACCTID";

    /// <summary>Account type tag (ACCTTYPE).</summary>
    public const string AccountType = "ACCTTYPE";

    /// <summary>Available balance tag (AVAILBAL).</summary>
    public const string AvailableBalance = "AVAILBAL";

    /// <summary>Balance amount tag (BALAMT).</summary>
    public const string BalanceAmount = "BALAMT";

    /// <summary>Bank account from tag (BANKACCTFROM).</summary>
    public const string BankAccountFrom = "BANKACCTFROM";

    /// <summary>Bank identifier tag (BANKID).</summary>
    public const string BankId = "BANKID";

    /// <summary>Bank transaction list tag (BANKTRANLIST).</summary>
    public const string BankTransactionList = "BANKTRANLIST";

    /// <summary>Check number tag (CHECKNUM).</summary>
    public const string CheckNumber = "CHECKNUM";

    /// <summary>Credit card account from tag (CCACCTFROM).</summary>
    public const string CreditCardAccountFrom = "CCACCTFROM";

    /// <summary>Credit card statement tag (CCSTMTRS).</summary>
    public const string CreditCardStatementRes = "CCSTMTRS";

    /// <summary>Credit card statement transactions response tag (CCSTMTTRNRS).</summary>
    public const string CreditCardStatementTransactionsRes = "CCSTMTTRNRS";

    /// <summary>Currency definition tag (CURDEF).</summary>
    public const string CurrencyDefinition = "CURDEF";

    /// <summary>Date as of tag (DTASOF).</summary>
    public const string DateAsOf = "DTASOF";

    /// <summary>Statement end date tag (DTEND).</summary>
    public const string DateEnd = "DTEND";

    /// <summary>Transaction posted date tag (DTPOSTED).</summary>
    public const string DatePosted = "DTPOSTED";

    /// <summary>Statement start date tag (DTSTART).</summary>
    public const string DateStart = "DTSTART";

    /// <summary>Transaction id tag (FITID).</summary>
    public const string TransactionId = "FITID";

    /// <summary>Ledger balance tag (LEDGERBAL).</summary>
    public const string LedgerBalance = "LEDGERBAL";

    /// <summary>Memo tag (MEMO).</summary>
    public const string Memo = "MEMO";

    /// <summary>Name tag (NAME).</summary>
    public const string Name = "NAME";

    /// <summary>Reference number tag (REFNUM).</summary>
    public const string ReferenceNumber = "REFNUM";

    /// <summary>Statement response tag (STMTRS).</summary>
    public const string StatementRes = "STMTRS";

    /// <summary>Statement transaction tag (STMTTRN).</summary>
    public const string StatementTransaction = "STMTTRN";

    /// <summary>Statement transactions response tag (STMTTRNRS).</summary>
    public const string StatementTransactionsRes = "STMTTRNRS";

    /// <summary>Transaction amount tag (TRNAMT).</summary>
    public const string TransactionAmount = "TRNAMT";

    /// <summary>Transaction type tag (TRNTYPE).</summary>
    public const string TransactionType = "TRNTYPE";

    /// <summary>Transaction unique id tag (TRNUID).</summary>
    public const string TransactionUid = "TRNUID";

    /// <summary>Bank messages response v1 tag (BANKMSGSRSV1).</summary>
    public const string BankMessagesResV1 = "BANKMSGSRSV1";

    /// <summary>Credit card messages response v1 tag (CREDITCARDMSGSRSV1).</summary>
    public const string CreditCardMessagesResV1 = "CREDITCARDMSGSRSV1";
}
