using Moq;
using Zylance.Contract.Api.Account;
using Zylance.Core.Router.Controllers;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Vault.Managers;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Tests.Router.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IAccountManager> _accountsMangerMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _accountsMangerMock = new();

        var vaultMock = VaultTestFactory.Create(_accountsMangerMock);
        var vaultContext = VaultContextTestFactory.Create(vaultMock);

        _controller = new AccountsController(vaultContext);
    }

    [Fact]
    public async Task ListAccounts_ReturnsAccounts()
    {
        // Arrange
        var accounts = new List<AccountModel>
        {
            new()
            {
                Id = "acc1",
                Name = "Account 1",
                Type = "Checking",
                Balance = 1000.00m,
                Currency = "CAD",
            },
            new()
            {
                Id = "acc2",
                Name = "Account 2",
                Type = "Checking",
                Balance = 1000.00m,
                Currency = "CAD",
            },
        };
        _accountsMangerMock.Setup(m => m.ListAsync()).Returns(Task.FromResult(accounts));

        var req = ZyRequestTestFactory.Create<ListAccountsReq>(new());
        var res = ZyResponseTestFactory.Create<ListAccountsRes>();

        // Act
        await _controller.ListAccounts(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(2, result.Accounts.Count);
        Assert.Contains(result.Accounts, a => a.Id == "acc1");
        Assert.Contains(result.Accounts, a => a.Id == "acc2");
    }

    [Fact]
    public async Task ListAccounts_WhenThereAreNoAccounts_ReturnsEmptyArray()
    {
        // Arrange
        var accounts = new List<AccountModel>();
        _accountsMangerMock.Setup(m => m.ListAsync()).Returns(Task.FromResult(accounts));

        var req = ZyRequestTestFactory.Create<ListAccountsReq>(new());
        var res = ZyResponseTestFactory.Create<ListAccountsRes>();

        // Act
        await _controller.ListAccounts(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Empty(result.Accounts);
    }

    [Fact]
    public async Task GetAccount_ReturnsAccount()
    {
        // Arrange
        var account = new AccountModel
        {
            Id = "acc1",
            Name = "Account 1",
            Type = "Checking",
            Balance = 1000.00m,
            Currency = "CAD",
        };
        _accountsMangerMock.Setup(m => m.GetAsync("acc1")).ReturnsAsync(account);

        var req = ZyRequestTestFactory.Create(new GetAccountReq { AccountId = "acc1" });
        var res = ZyResponseTestFactory.Create<GetAccountRes>();

        // Act
        await _controller.GetAccount(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal("acc1", result.Account.Id);
    }

    [Fact]
    public async Task GetAccount_WhenAccountDoesNotExist_ThrowsException()
    {
        // Arrange
        _accountsMangerMock.Setup(m => m.GetAsync("acc1")).ThrowsAsync(new Exception("Account not found"));

        var req = ZyRequestTestFactory.Create(new GetAccountReq { AccountId = "acc1" });
        var res = ZyResponseTestFactory.Create<GetAccountRes>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _controller.GetAccount(req, res));
    }

    [Fact]
    public async Task CreateAccount_CreatesAccount()
    {
        // Arrange
        var accountToCreate = new AccountModel
        {
            Id = "acc1",
            Name = "Account 1",
            Type = "Checking",
            Balance = 1000.00m,
            Currency = "CAD",
        };
        _accountsMangerMock.Setup(m => m.SaveAsync(It.IsAny<AccountModel>())).ReturnsAsync(accountToCreate);

        var req = ZyRequestTestFactory.Create(new CreateAccountReq { Account = AccountModel.ToData(accountToCreate) });
        var res = ZyResponseTestFactory.Create<CreateAccountRes>();

        // Act
        await _controller.CreateAccount(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal("acc1", result.Account.Id);
    }

    [Fact]
    public async Task UpdateAccount_UpdatesAccount()
    {
        // Arrange
        var updatedAccount = new AccountModel
        {
            Id = "acc1",
            Name = "Updated Account 1",
            Type = "Savings",
            Balance = 1500.00m,
            Currency = "CAD",
        };
        _accountsMangerMock.Setup(m => m.SaveAsync(It.IsAny<AccountModel>())).ReturnsAsync(updatedAccount);

        var req = ZyRequestTestFactory.Create(
            new UpdateAccountReq { AccountId = "acc1", Account = AccountModel.ToData(updatedAccount) }
        );
        var res = ZyResponseTestFactory.Create<UpdateAccountRes>();

        // Act
        await _controller.UpdateAccount(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal("Updated Account 1", result.Account.Name);
    }

    [Fact]
    public async Task UpdateAccount_WhenAccountDoesNotExist_ThrowsException()
    {
        // Arrange
        var updatedAccount = new AccountModel
        {
            Id = "acc1",
            Name = "Updated Account 1",
            Type = "Savings",
            Balance = 1500.00m,
            Currency = "CAD",
        };
        _accountsMangerMock.Setup(m => m.AssertExists("acc1")).ThrowsAsync(new Exception("Account not found"));

        var req = ZyRequestTestFactory.Create<UpdateAccountReq>(
            new() { AccountId = "acc1", Account = AccountModel.ToData(updatedAccount) }
        );
        var res = ZyResponseTestFactory.Create<UpdateAccountRes>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _controller.UpdateAccount(req, res));
    }

    [Fact]
    public async Task DeleteAccount_DeletesAccount()
    {
        // Arrange
        var accountToDelete = new AccountModel
        {
            Id = "acc1",
            Name = "Account 1",
            Type = "Checking",
            Balance = 1000.00m,
            Currency = "CAD",
        };
        _accountsMangerMock.Setup(m => m.GetAsync("acc1")).ReturnsAsync(accountToDelete);
        _accountsMangerMock.Setup(m => m.DeleteAsync("acc1")).Callback(() => { });
        var req = ZyRequestTestFactory.Create(new DeleteAccountReq { AccountId = "acc1" });
        var res = ZyResponseTestFactory.Create<DeleteAccountRes>();

        // Act
        await _controller.DeleteAccount(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.NotNull(result.Account);
        Assert.Equal("acc1", result.Account.Id);
    }

    [Fact]
    public async Task DeleteAccount_WhenAccountDoesNotExist_ThrowsException()
    {
        // Arrange
        _accountsMangerMock.Setup(m => m.GetAsync("acc1")).ThrowsAsync(new Exception("Account not found"));

        var req = ZyRequestTestFactory.Create(new DeleteAccountReq { AccountId = "acc1" });
        var res = ZyResponseTestFactory.Create<DeleteAccountRes>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _controller.DeleteAccount(req, res));
    }
}
