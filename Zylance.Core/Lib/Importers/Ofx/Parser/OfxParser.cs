using Zylance.Core.Lib.Importers.Ofx.Elements;
using Zylance.Core.Lib.Importers.Ofx.Models;
using Zylance.Core.Lib.Importers.Ofx.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Parser;

public class OfxParser
{
    private Func<OfxBankAccount, Task>? _accountHandler;
    private Func<OfxTransaction, Task>? _transactionHandler;
    private Func<OfxBalance, Task>? _balanceHandler;

    public void HandleAccount(Func<OfxBankAccount, Task> handler)
    {
        _accountHandler = handler;
    }

    public void HandleTransaction(Func<OfxTransaction, Task> handler)
    {
        _transactionHandler = handler;
    }

    public void HandleBalance(Func<OfxBalance, Task> handler)
    {
        _balanceHandler = handler;
    }

    public async Task ParseAsync(StreamReader content)
    {
        // Use the existing OfxRawFile parser to parse the entire structure
        var rawFile = OfxRawFile.Parse(content);
        
        // Extract currency from the statement response if it exists
        string? currency = null;
        
        // Walk the tree and emit events for specific elements
        await WalkElementTreeAsync(rawFile.Root, currency);
    }

    private async Task WalkElementTreeAsync(OfxRawElement element, string? currentCurrency)
    {
        // Extract currency if this is a statement response
        if (element.Name == "STMTRS" && element.Tokens.TryGetValue("CURDEF", out var curToken))
        {
            currentCurrency = curToken.Value;
        }

        // Emit bank account
        if (element.Name == "BANKACCTFROM")
        {
            await EmitBankAccountAsync(element, currentCurrency);
        }
        // Emit transaction
        else if (element.Name == "STMTTRN")
        {
            await EmitTransactionAsync(element);
        }
        // Emit ledger balance
        else if (element.Name == "LEDGERBAL")
        {
            await EmitBalanceAsync(element, "LEDGER");
        }
        // Emit available balance
        else if (element.Name == "AVAILBAL")
        {
            await EmitBalanceAsync(element, "AVAIL");
        }

        // Recursively process children
        foreach (var child in element.Children)
        {
            await WalkElementTreeAsync(child, currentCurrency);
        }
    }

    private async Task EmitBankAccountAsync(OfxRawElement element, string? currency)
    {
        if (_accountHandler is null)
            return;

        var bankId = element.Tokens.TryGetValue("BANKID", out var bankIdToken)
            ? bankIdToken.Value
            : throw new InvalidDataException("Missing BANKID in BANKACCTFROM");

        var accountId = element.Tokens.TryGetValue("ACCTID", out var acctIdToken)
            ? acctIdToken.Value
            : throw new InvalidDataException("Missing ACCTID in BANKACCTFROM");

        var accountType = element.Tokens.TryGetValue("ACCTTYPE", out var acctTypeToken)
            ? acctTypeToken.Value
            : throw new InvalidDataException("Missing ACCTTYPE in BANKACCTFROM");

        var account = new OfxBankAccount
        {
            BankId = bankId,
            AccountId = accountId,
            AccountType = accountType,
            Currency = currency,
        };

        await _accountHandler(account);
    }

    private async Task EmitTransactionAsync(OfxRawElement element)
    {
        if (_transactionHandler is null)
            return;

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

        var transaction = new OfxTransaction
        {
            Type = type,
            DatePosted = datePosted,
            Amount = amount,
            FitId = fitId,
            Name = element.Tokens.TryGetValue("NAME", out var nameToken) ? nameToken.Value : null,
            Memo = element.Tokens.TryGetValue("MEMO", out var memoToken) ? memoToken.Value : null,
            CheckNumber = element.Tokens.TryGetValue("CHECKNUM", out var checkToken) ? checkToken.Value : null,
            ReferenceNumber = element.Tokens.TryGetValue("REFNUM", out var refToken) ? refToken.Value : null,
        };

        await _transactionHandler(transaction);
    }

    private async Task EmitBalanceAsync(OfxRawElement element, string balanceType)
    {
        if (_balanceHandler is null)
            return;

        var amount = element.Tokens.TryGetValue("BALAMT", out var amtToken) && amtToken.DecimalValue.HasValue
            ? amtToken.DecimalValue.Value
            : throw new InvalidDataException($"Missing or invalid BALAMT in {balanceType}BAL");

        var asOfDate = element.Tokens.TryGetValue("DTASOF", out var dtAsOfToken) && dtAsOfToken.DateTimeValue.HasValue
            ? dtAsOfToken.DateTimeValue.Value
            : throw new InvalidDataException($"Missing or invalid DTASOF in {balanceType}BAL");

        var balance = new OfxBalance
        {
            Amount = amount,
            AsOfDate = asOfDate,
            Type = balanceType,
        };

        await _balanceHandler(balance);
    }

    public OfxFile Parse(StreamReader content)
    {
        var rawFile = OfxRawFile.Parse(content);
        return OfxFile.FromRaw(rawFile);
    }
}
