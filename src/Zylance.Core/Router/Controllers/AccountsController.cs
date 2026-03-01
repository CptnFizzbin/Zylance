using Serilog;
using Zylance.Contract.Api.Account;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Exceptions;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller that handles account-related requests
///     (create/read/update/delete/list).
/// </summary>
[Controller]
public class AccountsController(VaultContext vaultContext)
{
    private static readonly ILogger Log = ZyLogger.ForContext<AccountsController>();

    /// <summary>
    ///     Lists all accounts in the active vault.
    /// </summary>
    [RequestHandler]
    public async Task ListAccounts(ZyRequest<ListAccountsReq> req, ZyResponse<ListAccountsRes> res)
    {
        Log.Debug("ListAccounts called");
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        var accounts = await vault.Accounts.ListAsync();
        var resData = new ListAccountsRes();
        resData.Accounts.AddRange(accounts.Select(AccountModel.ToData));

        Log.Debug("ListAccounts returned {Count} accounts", accounts.Count);
        res.SetData(resData);
    }

    /// <summary>
    ///     Retrieves an account by id from the active vault.
    /// </summary>
    [RequestHandler]
    public async Task GetAccount(ZyRequest<GetAccountReq> req, ZyResponse<GetAccountRes> res)
    {
        var data = req.GetData();
        Log.Debug("GetAccount called Id={Id}", data.AccountId);
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();
        if (string.IsNullOrWhiteSpace(data.AccountId))
            throw new ArgumentException("AccountId is required");

        var account = await vault.Accounts.GetAsync(data.AccountId);
        res.SetData(new GetAccountRes { Account = AccountModel.ToData(account) });
        Log.Debug("GetAccount returned AccountId={AccountId}", account.Id);
    }

    /// <summary>
    ///     Creates a new account in the active vault.
    /// </summary>
    [RequestHandler]
    public async Task CreateAccount(ZyRequest<CreateAccountReq> req, ZyResponse<CreateAccountRes> res)
    {
        var data = req.GetData();
        Log.Debug("CreateAccount called");
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();
        await vault.WithScope(async scope =>
        {
            var model = AccountModel.FromData(data.Account);
            var savedAccount = await scope.Vault.Accounts.SaveAsync(model);
            res.SetData(new CreateAccountRes { Account = AccountModel.ToData(savedAccount) });
            Log.Information("Created account {AccountId}", savedAccount.Id);
        });
    }

    /// <summary>
    ///     Updates an existing account in the active vault.
    /// </summary>
    [RequestHandler]
    public async Task UpdateAccount(ZyRequest<UpdateAccountReq> req, ZyResponse<UpdateAccountRes> res)
    {
        var data = req.GetData();
        Log.Debug("UpdateAccount called Id={Id}", data.AccountId);
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();

        if (string.IsNullOrWhiteSpace(data.AccountId))
            throw new ArgumentException("AccountId is required");

        if (data.Account.Id != data.AccountId)
            throw new ArgumentException("Account ID mismatch between URL and payload");

        await vault.Accounts.AssertExists(data.AccountId);

        await vault.WithScope(async scope =>
        {
            var model = AccountModel.FromData(data.Account);
            var updatedAccount = await scope.Vault.Accounts.SaveAsync(model);
            res.SetData(new UpdateAccountRes { Account = AccountModel.ToData(updatedAccount) });
            Log.Information("Updated account {AccountId}", updatedAccount.Id);
        });
    }

    /// <summary>
    ///     Deletes an account by id from the active vault.
    /// </summary>
    [RequestHandler]
    public async Task DeleteAccount(ZyRequest<DeleteAccountReq> req, ZyResponse<DeleteAccountRes> res)
    {
        var data = req.GetData();
        Log.Debug("DeleteAccount called Id={Id}", data.AccountId);
        var vault = vaultContext.ActiveVault ?? throw VaultException.NoActiveVault();
        if (string.IsNullOrWhiteSpace(data.AccountId))
            throw new ArgumentException("AccountId is required");

        await vault.WithScope(async scope =>
        {
            var account = await scope.Vault.Accounts.GetAsync(data.AccountId);
            await scope.Vault.Accounts.DeleteAsync(account.Id);
            res.SetData(new DeleteAccountRes { Account = AccountModel.ToData(account) });
            Log.Information("Deleted account {AccountId}", data.AccountId);
        });
    }
}
