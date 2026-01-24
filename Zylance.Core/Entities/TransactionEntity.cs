using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zylance.Core.Models;

namespace Zylance.Core.Entities;

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
    ///     Debit amount stored in cents.
    ///     Null if this is a credit transaction.
    /// </summary>
    [Column(TypeName = "INTEGER")]
    public MonetaryValue? Debit { get; set; }

    /// <summary>
    ///     Credit amount stored in cents.
    ///     Null if this is a debit transaction.
    /// </summary>
    [Column(TypeName = "INTEGER")]
    public MonetaryValue? Credit { get; set; }
}
