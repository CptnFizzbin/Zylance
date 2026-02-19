using System.Globalization;
using Zylance.Contract.Models.Ledger;

namespace Zylance.Core.Vault.Models;

/// <summary>
///     Represents a single ledger entry / transaction stored in the vault.
/// </summary>
public record LedgerEntryModel
{
    /// <summary>
    ///     Unique identifier for the ledger entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Optional transaction identifier linking this ledger entry to an
    ///     external/imported transaction.
    /// </summary>
    public string? TrxId { get; init; }

    /// <summary>
    ///     Identifier of the account this entry belongs to.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    ///     Timestamp when the transaction was posted.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    ///     Payee or counterparty for the transaction.
    /// </summary>
    public required string Payee { get; init; }

    /// <summary>
    ///     Optional memo or description for the transaction.
    /// </summary>
    public required string Memo { get; init; }

    /// <summary>
    ///     Transaction amount (positive for credits, negative for debits depending
    ///     on ledger conventions).
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    ///     Converts the model to a data transfer object for storage or transmission.
    /// </summary>
    /// <returns>DTO of the LedgerEntry</returns>
    public static LedgerEntryData ToData(LedgerEntryModel model)
    {
        return new LedgerEntryData
        {
            Id = model.Id.ToString(),
            AccountId = model.AccountId,
            Timestamp = model.Timestamp.ToString("O"),
            Payee = model.Payee,
            Memo = model.Memo,
            Amount = model.Amount.ToString("F2", CultureInfo.InvariantCulture),
            TrxId = model.TrxId ?? string.Empty,
        };
    }

    /// <summary>
    ///     Converts a data transfer object to a LedgerEntryModel for use in the
    ///     application.
    /// </summary>
    /// <param name="data">DTO of the LedgerEntry</param>
    /// <returns>Model for the LedgerEntry</returns>
    public static LedgerEntryModel FromData(LedgerEntryData data)
    {
        return new LedgerEntryModel
        {
            Id = new Guid(data.Id),
            AccountId = data.AccountId,
            Timestamp = DateTimeOffset.Parse(data.Timestamp),
            Payee = data.Payee,
            Memo = data.Memo,
            Amount = decimal.Parse(data.Amount, CultureInfo.InvariantCulture),
            TrxId = string.IsNullOrEmpty(data.TrxId) ? null : data.TrxId,
        };
    }
}
