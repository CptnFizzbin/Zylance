using Zylance.Core.Vault.Models;

namespace Zylance.Core.Importers.Models;

/// <summary>
///     Represents the imported statement for a single account, including the
///     account metadata and the list of ledger transactions parsed for that
///     account.
/// </summary>
public record ImportStatement
{
    /// <summary>
    ///     The account the statement belongs to.
    /// </summary>
    public required AccountModel Account { get; init; }

    /// <summary>
    ///     The transactions parsed for the account in this statement.
    /// </summary>
    public required IReadOnlyList<LedgerEntryModel> Transactions { get; init; }
}
