using System.ComponentModel.DataAnnotations;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing a ledger entry in the local vault.
/// </summary>
public class LedgerEntryEntity
{
    [Key]
    public required Guid Id { get; init; }

    public required Guid AccountId { get; set; }

    public required long Timestamp { get; set; }

    [MaxLength(255)]
    public required string Payee { get; set; }

    [MaxLength(255)]
    public required string Memo { get; set; }

    public required double Amount { get; set; }
}
