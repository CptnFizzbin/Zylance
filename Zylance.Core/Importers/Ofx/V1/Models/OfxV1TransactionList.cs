using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Importers.Ofx.V1.Models;

internal record OfxV1TransactionList
{
    public required DateTimeOffset StartDate { get; init; }
    public required DateTimeOffset EndDate { get; init; }
    public required List<OfxTransaction> Transactions { get; init; }

    public static OfxV1TransactionList From(OfxRawElement element)
    {
        if (element.Name != OfxTagNames.BankTransactionList)
            throw new InvalidDataException($"Invalid element {element.Name} for OfxTransactionList");

        return new OfxV1TransactionList
        {
            StartDate = element.GetToken(OfxTagNames.DateStart).DateTimeValue,
            EndDate = element.GetToken(OfxTagNames.DateEnd).DateTimeValue,
            Transactions = element
                .Children.Where(c => c.Name == OfxTagNames.StatementTransaction)
                .Select(OfxV1Transaction.From)
                .ToList(),
        };
    }
}
