namespace Zylance.Core.Importers.Ofx.Models;

public record OfxBalance
{
    public required decimal Amount { get; init; }
    public required DateTimeOffset AsOfDate { get; init; }
    public required string Type { get; init; } // "LEDGER" or "AVAIL"
}
