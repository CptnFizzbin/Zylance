using Moq;
using Zylance.Contract.Api.Ledger;
using Zylance.Contract.Models.Ledger;
using Zylance.Core.Router.Controllers;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Vault.Managers;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Tests.Router.Controllers;

public class LedgerControllerTests
{
    private readonly LedgerController _controller;
    private readonly Mock<ILedgerManager> _ledgerManagerMock;

    public LedgerControllerTests()
    {
        _ledgerManagerMock = new Mock<ILedgerManager>();

        var vaultMock = VaultTestFactory.Create(ledgerManagerMock: _ledgerManagerMock);

        var context = VaultContextTestFactory.Create(vaultMock);
        _controller = new LedgerController(context);
    }

    [Fact]
    public async Task CreateLedgerEntry_SavesEntryAndReturnsResult()
    {
        // Arrange
        var entryData = new LedgerEntryData
        {
            Id = Guid.NewGuid().ToString(),
            AccountId = "acc-1",
            Timestamp = DateTimeOffset.UtcNow.ToString("O"),
            Payee = "Payee",
            Memo = "Memo",
            Amount = "100.00",
            TrxId = "trx-1",
        };
        var entryModel = LedgerEntryModel.FromData(entryData);
        var savedModel = entryModel with { Id = Guid.NewGuid() };
        _ledgerManagerMock.Setup(m => m.SaveAsync(It.IsAny<LedgerEntryModel>())).ReturnsAsync(savedModel);

        var req = ZyRequestTestFactory.Create(new CreateLedgerEntryReq { Entry = entryData });
        var res = ZyResponseTestFactory.Create<CreateLedgerEntryRes>();

        // Act
        await _controller.CreateLedgerEntry(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(savedModel.Id.ToString(), result.Entry.Id);
        Assert.Equal(savedModel.AccountId, result.Entry.AccountId);
        Assert.Equal(savedModel.Payee, result.Entry.Payee);
        Assert.Equal(savedModel.Memo, result.Entry.Memo);
        Assert.Equal(savedModel.Amount.ToString("F2"), result.Entry.Amount);
    }

    [Fact]
    public async Task GetLedgerEntry_ReturnsEntry()
    {
        // Arrange
        var entryModel = new LedgerEntryModel
        {
            Id = Guid.NewGuid(),
            AccountId = "acc-1",
            Timestamp = DateTimeOffset.UtcNow,
            Payee = "Payee",
            Memo = "Memo",
            Amount = 100.00m,
            TrxId = "trx-1",
        };
        _ledgerManagerMock.Setup(m => m.GetAsync(entryModel.Id)).ReturnsAsync(entryModel);

        var req = ZyRequestTestFactory.Create(new GetLedgerEntryReq { Id = entryModel.Id.ToString() });
        var res = ZyResponseTestFactory.Create<GetLedgerEntryRes>();

        // Act
        await _controller.GetLedgerEntry(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Equal(entryModel.Id.ToString(), result.Entry.Id);
        Assert.Equal(entryModel.AccountId, result.Entry.AccountId);
        Assert.Equal(entryModel.Payee, result.Entry.Payee);
        Assert.Equal(entryModel.Memo, result.Entry.Memo);
        Assert.Equal(entryModel.Amount.ToString("F2"), result.Entry.Amount);
    }

    [Fact]
    public async Task ListLedgerEntries_ReturnsEntries()
    {
        // Arrange
        var entryModel = new LedgerEntryModel
        {
            Id = Guid.NewGuid(),
            AccountId = "acc-1",
            Timestamp = DateTimeOffset.UtcNow,
            Payee = "Payee",
            Memo = "Memo",
            Amount = 100.00m,
            TrxId = "trx-1",
        };
        var cursorList = new CursorList<LedgerEntryModel>
        {
            Cursor = "cursor",
            TotalCount = 1,
            Items = [entryModel],
            NextPage = null,
        };
        _ledgerManagerMock.Setup(m => m.ListAsync(It.IsAny<LedgerFilter>())).ReturnsAsync(cursorList);

        var req = ZyRequestTestFactory.Create(new ListLedgerEntriesReq { Filter = null });
        var res = ZyResponseTestFactory.Create<ListLedgerEntriesRes>();

        // Act
        await _controller.ListLedgerEntries(req, res);

        // Assert
        var result = res.GetData();
        Assert.NotNull(result);
        Assert.Single(result.Entries);
        Assert.Equal(entryModel.Id.ToString(), result.Entries[0].Id);
        Assert.Equal(cursorList.TotalCount, result.TotalCount);
        Assert.Equal(cursorList.Cursor, result.Cursor);
        Assert.True(result.LastPage);
    }
}
