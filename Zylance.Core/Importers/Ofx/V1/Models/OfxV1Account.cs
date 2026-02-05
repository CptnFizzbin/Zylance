using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal static class OfxV1Account
{
    public static OfxAccount From(OfxRawElement element, string currency)
    {
        var accountType = element.Name switch
        {
            OfxTagNames.BankAccountFrom => element.GetToken(OfxTagNames.AccountType).Value,
            OfxTagNames.CreditCardAccountFrom => OfxAccountType.CreditCard,
            _ => throw new InvalidDataException($"Invalid element {element.Name} for OfxAccount"),
        };

        return new OfxAccount
        {
            AccountType = accountType,
            AccountId = element.GetToken(OfxTagNames.AccountId).Value,
            Currency = currency,
            BankId = element.TryGetToken(OfxTagNames.BankId, out var bankId) ? bankId.Value : null,
        };
    }
}
