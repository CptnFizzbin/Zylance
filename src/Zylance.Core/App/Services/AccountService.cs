using Zylance.Contract.Models.Account;

namespace Zylance.Core.App.Services;

/// <summary>
///     Service for managing accounts within vaults.
/// </summary>
public class AccountService
{
    /// <summary>
    /// Creates a new account DTO with the provided values.
    /// </summary>
    /// <param name="name">Account display name.</param>
    /// <param name="type">Account type identifier.</param>
    /// <param name="balance">Initial balance.</param>
    public AccountData CreateAccount(string name, string type, double balance = 0.0)
    {
        return new AccountData
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = type,
            Balance = balance,
        };
    }

    /// <summary>
    /// Retrieves an account DTO by id. Not yet implemented; placeholder for vault-backed retrieval.
    /// </summary>
    /// <param name="accountId">Identifier of the account to retrieve.</param>
    public AccountData GetAccount(string accountId)
    {
        // TODO: Implement actual retrieval from vault
        throw new NotImplementedException();
    }
}
