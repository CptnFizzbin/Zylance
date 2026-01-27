using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zylance.Vault.Local.Entities;

public class TransactionEntity
{
    /// <summary>
    ///     Primary key using UUIDv7 for time-ordered globally unique identifiers.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     The payee for this transaction.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string Payee { get; set; }

    /// <summary>
    ///     Optional memo or note about the transaction.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public required string Memo { get; set; }

    /// <summary>
    ///     Debit amount in dollars.
    ///     Null if this is a credit transaction.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Debit { get; set; }

    /// <summary>
    ///     Credit amount in dollars.
    ///     Null if this is a debit transaction.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Credit { get; set; }
}
