using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal static class OfxV1Balance
{
    public static OfxBalance From(OfxRawElement element)
    {
        var type = element.Name switch
        {
            OfxTagNames.LedgerBalance => "LEDGER",
            OfxTagNames.AvailableBalance => "AVAIL",
            _ => throw new InvalidDataException(
                $"Element is not {OfxTagNames.LedgerBalance} or {OfxTagNames.AvailableBalance}"
            ),
        };

        return new OfxBalance
        {
            Type = type,
            Amount = element.GetToken(OfxTagNames.BalanceAmount).DecimalValue,
            AsOfDate = element.GetToken(OfxTagNames.DateAsOf).DateTimeValue,
        };
    }
}
