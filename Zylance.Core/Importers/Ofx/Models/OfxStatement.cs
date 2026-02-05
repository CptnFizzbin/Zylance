namespace Zylance.Core.Importers.Ofx.Models;

public record OfxStatement
{
    public required OfxAccount Account { get; init; }
    public required OfxBalance LedgerBalance { get; init; }
    public OfxBalance? AvailableBalance { get; init; }
    public required List<OfxTransaction> Transactions { get; init; }
    public DateTimeOffset? DateStart { get; init; }
    public DateTimeOffset? DateEnd { get; init; }
}
