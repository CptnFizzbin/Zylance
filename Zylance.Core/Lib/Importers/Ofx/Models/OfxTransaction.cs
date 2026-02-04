namespace Zylance.Core.Lib.Importers.Ofx.Models;

public record OfxTransaction
{
    public required string Type { get; init; }
    public required DateTimeOffset DatePosted { get; init; }
    public required decimal Amount { get; init; }
    public required string FitId { get; init; }
    public string? Name { get; init; }
    public string? Memo { get; init; }
    public string? CheckNumber { get; init; }
    public string? ReferenceNumber { get; init; }
}
