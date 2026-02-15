using System.ComponentModel.DataAnnotations;
using Zylance.Core.Vault.Models;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing a ledger entry in the local vault.
/// </summary>
public class LedgerEntryEntity
{
    /// <summary>
    ///     Unique identifier for the ledger entry.
    /// </summary>
    [Key]
    public required Guid Id { get; init; }

    /// <summary>
    ///     Identifier of the account associated with this ledger entry.
    /// </summary>
    public required string AccountId { get; set; }

    /// <summary>
    ///     Unix timestamp (milliseconds) for when the entry occurred.
    /// </summary>
    public required DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     Payee or counterparty for the ledger entry.
    /// </summary>
    [MaxLength(255)]
    public required string Payee { get; set; }

    /// <summary>
    ///     Optional memo or description for the ledger entry.
    /// </summary>
    [MaxLength(255)]
    public required string Memo { get; set; }

    /// <summary>
    ///     Optional transaction identifier linking this ledger entry to an external/imported transaction.
    /// </summary>
    [MaxLength(255)]
    public string? TrxId { get; set; }

    /// <summary>
    ///     Monetary amount for the ledger entry (positive or negative).
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    ///     Converts a LedgerEntryEntity to LedgerEntryModel.
    /// </summary>
    public static LedgerEntryModel ToModel(LedgerEntryEntity entity)
    {
        return new LedgerEntryModel
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            Timestamp = entity.Timestamp,
            Payee = entity.Payee,
            Memo = entity.Memo,
            Amount = entity.Amount,
            TrxId = entity.TrxId,
        };
    }
}
