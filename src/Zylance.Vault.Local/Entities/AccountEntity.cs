using System.ComponentModel.DataAnnotations;
using Zylance.Core.Vault.Models;

namespace Zylance.Vault.Local.Entities;

/// <summary>
///     Entity Framework entity representing an account in the local vault.
/// </summary>
public class AccountEntity
{
    /// <summary>
    ///     Unique identifier for the account.
    /// </summary>
    [Key]
    [MaxLength(255)]
    public required string Id { get; init; }

    /// <summary>
    ///     Human-friendly account name.
    /// </summary>
    [MaxLength(255)]
    public required string Name { get; set; }

    /// <summary>
    ///     Account type or category (e.g. Checking, Savings).
    /// </summary>
    [MaxLength(255)]
    public required string Type { get; set; }

    /// <summary>
    ///     Current balance of the account.
    /// </summary>
    public required decimal Balance { get; set; }

    /// <summary>
    ///     Converts a AccountEntity to AccountModel.
    /// </summary>
    public static AccountModel ToModel(AccountEntity arg)
    {
        return new()
        {
            Id = arg.Id,
            Name = arg.Name,
            Type = arg.Type,
            Balance = arg.Balance,
        };
    }
}
