using Zylance.Contract.Models.Account;

namespace Zylance.Core.App.Services;

/// <summary>
///     Service for managing accounts within vaults.
/// </summary>
public class AccountService
{
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

    public AccountData GetAccount(string accountId)
    {
        // TODO: Implement actual retrieval from vault
        throw new NotImplementedException();
    }
}
