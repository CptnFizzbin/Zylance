using System.ComponentModel.DataAnnotations;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing an account in the local vault.
/// </summary>
public class AccountEntity
{
    [Key]
    public required Guid Id { get; init; }

    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Type { get; set; }

    public required double Balance { get; set; }
}
