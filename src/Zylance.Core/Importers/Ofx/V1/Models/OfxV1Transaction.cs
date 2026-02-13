using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal abstract record OfxV1Transaction : OfxTransaction
{
    public static OfxTransaction From(OfxRawElement element)
    {
        if (element.Name != OfxTagNames.StatementTransaction)
            throw new InvalidDataException($"Invalid element {element.Name} for OfxTransaction");

        var isTransfer = element.GetToken(OfxTagNames.TransactionType).Value.Equals(OfxTransactionTypes.Transfer);

        return new OfxTransaction
        {
            Id = element.GetToken(OfxTagNames.TransactionId).Value,
            Type = element.GetToken(OfxTagNames.TransactionType).Value,
            DatePosted = element.GetToken(OfxTagNames.DatePosted).DateTimeValue,
            Amount = element.GetToken(OfxTagNames.TransactionAmount).DecimalValue,
            Name = element.GetToken(OfxTagNames.Name).Value,
            Memo = element.GetToken(OfxTagNames.Memo).Value,
            CheckNumber = element.TryGetToken(OfxTagNames.CheckNumber, out var checkNum) ? checkNum.Value : null,
            ReferenceNumber = element.TryGetToken(OfxTagNames.ReferenceNumber, out var refNum) ? refNum.Value : null,
            IsTransfer = isTransfer,
        };
    }
}
