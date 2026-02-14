using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal static class OfxV1Statement
{
    public static OfxStatement From(OfxRawElement element)
    {
        if (
            element.Name != OfxTagNames.StatementTransactionsRes
            && element.Name != OfxTagNames.CreditCardStatementTransactionsRes
        )
            throw new InvalidDataException(
                $"Element is not {OfxTagNames.StatementTransactionsRes} or {OfxTagNames.CreditCardStatementTransactionsRes}"
            );

        // Determine statement type and get appropriate child elements
        var isCreditCard = element.Name == OfxTagNames.CreditCardStatementTransactionsRes;
        var statementResTag = isCreditCard ? OfxTagNames.CreditCardStatementRes : OfxTagNames.StatementRes;
        var accountFromTag = isCreditCard ? OfxTagNames.CreditCardAccountFrom : OfxTagNames.BankAccountFrom;

        var statementRes = element.GetChildElement(statementResTag);
        var currency = statementRes.GetToken(OfxTagNames.CurrencyDefinition).Value;

        var accountElm = statementRes.GetChildElement(accountFromTag);
        var account = OfxV1Account.From(accountElm, currency);

        var ledgerBalanceElm = statementRes.GetChildElement(OfxTagNames.LedgerBalance);
        var ledgerBalance = OfxV1Balance.From(ledgerBalanceElm);

        OfxBalance? availableBalance = null;
        if (statementRes.TryGetChildElement(OfxTagNames.AvailableBalance, out var availableBalanceElm))
            availableBalance = OfxV1Balance.From(availableBalanceElm);

        var transactionList = OfxV1TransactionList.From(statementRes.GetChildElement(OfxTagNames.BankTransactionList));

        return new OfxStatement
        {
            Account = account,
            DateStart = transactionList.StartDate,
            DateEnd = transactionList.EndDate,
            Transactions = transactionList.Transactions,
            LedgerBalance = ledgerBalance,
            AvailableBalance = availableBalance,
        };
    }
}
