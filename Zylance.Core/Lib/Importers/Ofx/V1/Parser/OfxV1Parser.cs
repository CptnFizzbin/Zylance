using Zylance.Core.Lib.Importers.Ofx.Elements;
using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.V1.Parser;

/// <summary>
/// Parses OFX V1 (SGML) format files and returns structured statement data.
/// </summary>
public class OfxV1Parser
{
    /// <summary>
    /// Parses an OFX V1 file and returns a list of statements.
    /// Each statement contains an account, balance information, and transactions.
    /// </summary>
    /// <param name="content">StreamReader containing the OFX file content</param>
    /// <returns>List of OFX statements parsed from the file</returns>
    public async Task<List<OfxStatement>> ParseAsync(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        
        var statements = new List<OfxStatement>();
        await ExtractStatementsAsync(rawFile.Root, statements);
        
        return statements;
    }

    private async Task ExtractStatementsAsync(OfxRawElement element, List<OfxStatement> statements)
    {
        if (element.Name == "STMTTRNRS")
        {
            var statement = await BuildStatementAsync(element);
            if (statement is not null)
            {
                statements.Add(statement);
            }
        }

        foreach (var child in element.Children)
        {
            await ExtractStatementsAsync(child, statements);
        }
    }

    private async Task<OfxStatement?> BuildStatementAsync(OfxRawElement stmtTrnRs)
    {
        var stmtRsElement = stmtTrnRs.Children.FirstOrDefault(c => c.Name == "STMTRS");
        if (stmtRsElement is null)
            return null;

        var currency = stmtRsElement.Tokens.TryGetValue("CURDEF", out var curToken)
            ? curToken.Value
            : null;

        var bankAcctFromElement = stmtRsElement.Children.FirstOrDefault(c => c.Name == "BANKACCTFROM");
        if (bankAcctFromElement is null)
            return null;

        var account = BuildBankAccount(bankAcctFromElement, currency);

        var ledgerBalElement = stmtRsElement.Children.FirstOrDefault(c => c.Name == "LEDGERBAL");
        if (ledgerBalElement is null)
            return null;

        var ledgerBalance = BuildBalance(ledgerBalElement, "LEDGER");

        var availBalElement = stmtRsElement.Children.FirstOrDefault(c => c.Name == "AVAILBAL");
        var availableBalance = availBalElement is not null
            ? BuildBalance(availBalElement, "AVAIL")
            : null;

        var transactions = new List<OfxTransaction>();
        var bankTranListElement = stmtRsElement.Children.FirstOrDefault(c => c.Name == "BANKTRANLIST");
        if (bankTranListElement is not null)
        {
            foreach (var stmtTrnElement in bankTranListElement.Children.Where(c => c.Name == "STMTTRN"))
            {
                var transaction = BuildTransaction(stmtTrnElement);
                transactions.Add(transaction);
            }
        }

        return new OfxStatement
        {
            Account = account,
            LedgerBalance = ledgerBalance,
            AvailableBalance = availableBalance,
            Transactions = transactions,
        };
    }

    private OfxBankAccount BuildBankAccount(OfxRawElement element, string? currency)
    {
        var bankId = element.Tokens.TryGetValue("BANKID", out var bankIdToken)
            ? bankIdToken.Value
            : throw new InvalidDataException("Missing BANKID in BANKACCTFROM");

        var accountId = element.Tokens.TryGetValue("ACCTID", out var acctIdToken)
            ? acctIdToken.Value
            : throw new InvalidDataException("Missing ACCTID in BANKACCTFROM");

        var accountType = element.Tokens.TryGetValue("ACCTTYPE", out var acctTypeToken)
            ? acctTypeToken.Value
            : throw new InvalidDataException("Missing ACCTTYPE in BANKACCTFROM");

        return new OfxBankAccount
        {
            BankId = bankId,
            AccountId = accountId,
            AccountType = accountType,
            Currency = currency,
            Type = "BANK",
        };
    }

    private OfxTransaction BuildTransaction(OfxRawElement element)
    {
        var type = element.Tokens.TryGetValue("TRNTYPE", out var typeToken)
            ? typeToken.Value
            : throw new InvalidDataException("Missing TRNTYPE in STMTTRN");

        var datePosted = element.Tokens.TryGetValue("DTPOSTED", out var dtPostedToken) && dtPostedToken.DateTimeValue.HasValue
            ? dtPostedToken.DateTimeValue.Value
            : throw new InvalidDataException("Missing or invalid DTPOSTED in STMTTRN");

        var amount = element.Tokens.TryGetValue("TRNAMT", out var amtToken) && amtToken.DecimalValue.HasValue
            ? amtToken.DecimalValue.Value
            : throw new InvalidDataException("Missing or invalid TRNAMT in STMTTRN");

        var fitId = element.Tokens.TryGetValue("FITID", out var fitIdToken)
            ? fitIdToken.Value
            : throw new InvalidDataException("Missing FITID in STMTTRN");

        var isTransfer = type.Equals("XFER", StringComparison.OrdinalIgnoreCase);

        return new OfxTransaction
        {
            Type = type,
            DatePosted = datePosted,
            Amount = amount,
            FitId = fitId,
            Name = element.Tokens.TryGetValue("NAME", out var nameToken) ? nameToken.Value : null,
            Memo = element.Tokens.TryGetValue("MEMO", out var memoToken) ? memoToken.Value : null,
            CheckNumber = element.Tokens.TryGetValue("CHECKNUM", out var checkToken) ? checkToken.Value : null,
            ReferenceNumber = element.Tokens.TryGetValue("REFNUM", out var refToken) ? refToken.Value : null,
            IsTransfer = isTransfer,
        };
    }

    private OfxBalance BuildBalance(OfxRawElement element, string balanceType)
    {
        var amount = element.Tokens.TryGetValue("BALAMT", out var amtToken) && amtToken.DecimalValue.HasValue
            ? amtToken.DecimalValue.Value
            : throw new InvalidDataException($"Missing or invalid BALAMT in {balanceType}BAL");

        var asOfDate = element.Tokens.TryGetValue("DTASOF", out var dtAsOfToken) && dtAsOfToken.DateTimeValue.HasValue
            ? dtAsOfToken.DateTimeValue.Value
            : throw new InvalidDataException($"Missing or invalid DTASOF in {balanceType}BAL");

        return new OfxBalance
        {
            Amount = amount,
            AsOfDate = asOfDate,
            Type = balanceType,
        };
    }

    public OfxFile Parse(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        return OfxFile.FromRaw(rawFile);
    }
}
