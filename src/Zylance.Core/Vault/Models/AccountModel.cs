using System.Globalization;
using Zylance.Contract.Models.Account;

namespace Zylance.Core.Vault.Models;

/// <summary>
///     Represents a financial account as stored in the vault.
/// </summary>
public record AccountModel
{
    /// <summary>
    ///     Unique identifier for the account.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     Human-readable account name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The account type (e.g., "checking", "savings", "credit").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    ///     Current ledger balance for the account.
    /// </summary>
    public required decimal Balance { get; init; }

    /// <summary>
    ///     Currency code for the account (e.g., "USD").
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    ///     Available balance (maybe null if not applicable).
    /// </summary>
    public decimal? AvailableBalance { get; init; }

    /// <summary>
    ///     Converts an AccountData object to an AccountModel for application use.
    /// </summary>
    /// <param name="account">The DTO of the account</param>
    /// <returns>A model for application use</returns>
    public static AccountModel FromData(AccountData account)
    {
        return new()
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = decimal.Parse(account.Balance),
            Currency = account.Currency,
            AvailableBalance = decimal.TryParse(account.AvailableBalance, out var availableBalance)
                ? availableBalance
                : null,
        };
    }

    /// <summary>
    ///     Converts an AccountModel to an AccountData object for transmission.
    /// </summary>
    /// <param name="account">The model for the account</param>
    /// <returns>The DTO of the account</returns>
    public static AccountData ToData(AccountModel account)
    {
        return new()
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            Balance = account.Balance.ToString(CultureInfo.InvariantCulture),
            Currency = account.Currency,
            AvailableBalance = account.AvailableBalance?.ToString(CultureInfo.InvariantCulture) ?? "",
        };
    }
}
