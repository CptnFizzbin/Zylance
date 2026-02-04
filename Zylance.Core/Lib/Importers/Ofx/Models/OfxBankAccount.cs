namespace Zylance.Core.Lib.Importers.Ofx.Models;

public record OfxBankAccount
{
    public required string BankId { get; init; }
    public required string AccountId { get; init; }
    public required string AccountType { get; init; }
    public string? Currency { get; init; }
}
