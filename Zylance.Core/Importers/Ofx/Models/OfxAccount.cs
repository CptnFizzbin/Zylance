namespace Zylance.Core.Importers.Ofx.Models;

public record OfxAccount
{
    public required string AccountId { get; init; }
    public required string? AccountType { get; init; }
    public required string Currency { get; init; }
    public string? BankId { get; init; }
}
