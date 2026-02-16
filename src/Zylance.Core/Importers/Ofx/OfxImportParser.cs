using Zylance.Core.Importers.Interfaces;
using Zylance.Core.Importers.Models;
using Zylance.Core.Importers.Ofx.Models;
using Zylance.Core.Importers.Ofx.V1;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Importers.Ofx;

/// <summary>
///     Importer for OFX (Open Financial Exchange) files.
/// </summary>
public class OfxImportParser : IImportParser
{
    /// <summary>
    ///     Supported file extensions and display names for this importer.
    /// </summary>
    public IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; } =
    [("Open Financial Exchange", [".ofx"]), ("Quicken Financial Exchange", [".qfx"])];

    /// <summary>
    ///     Imports the provided file and returns an ImportResult.
    /// </summary>
    public Task<ParseResult> ParseAsync(Stream file, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(file);

        // TODO: Implement version detection logic
        var statements = OfxV1Parser.Parse(reader);
        var transactionCount = statements.Sum(s => s.Transactions.Count);

        return Task.FromResult(
            new ParseResult
            {
                Success = true,
                TransactionCount = transactionCount,
                Statements = [.. statements.Select(ToImportStatement)],
            }
        );
    }

    private static ImportStatement ToImportStatement(OfxStatement statement)
    {
        var account = new AccountModel
        {
            Id = statement.Account.AccountId,
            Name = statement.Account.AccountId,
            Type = statement.Account.AccountType,
            Currency = statement.Account.Currency,
            Balance = statement.LedgerBalance.Amount,
            AvailableBalance = statement.AvailableBalance?.Amount,
        };

        var transactions = statement.Transactions.Select(entry => ToLedgerEntry(account, entry)).ToList();

        return new ImportStatement { Account = account, Transactions = transactions };
    }

    private static LedgerEntryModel ToLedgerEntry(AccountModel account, OfxTransaction transaction)
    {
        return new LedgerEntryModel
        {
            Id = Guid.CreateVersion7(transaction.DatePosted),
            AccountId = account.Id,
            Timestamp = transaction.DatePosted,
            Payee = transaction.Name ?? string.Empty,
            Memo = transaction.Memo ?? string.Empty,
            TrxId = transaction.Id,
            Amount = transaction.Amount,
        };
    }
}
