using Moq;
using Zylance.Core.Vault.Interfaces;
using Zylance.Core.Vault.Managers;

namespace Zylance.Core.Tests.TestUtils.Factories.Models;

public static class VaultTestFactory
{
    public static Mock<IVault> Create(
        Mock<IAccountManager>? accountManagerMock = null,
        Mock<ILedgerManager>? ledgerManagerMock = null
    )
    {
        var vaultMock = new Mock<IVault>();

        var accountMgr = accountManagerMock ?? new Mock<IAccountManager>();
        vaultMock.SetupGet(v => v.Accounts).Returns(accountMgr.Object);

        var ledgerMgr = ledgerManagerMock ?? new Mock<ILedgerManager>();
        vaultMock.SetupGet(v => v.Ledgers).Returns(ledgerMgr.Object);

        var vaultScopeMock = new Mock<IVaultScope>();
        vaultScopeMock.SetupGet(s => s.Vault).Returns(vaultMock.Object);
        vaultScopeMock.Setup(s => s.Commit()).Returns(Task.CompletedTask);
        vaultScopeMock.Setup(s => s.Rollback()).Returns(Task.CompletedTask);
        vaultScopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

        vaultMock.Setup(v => v.CreateScope()).Returns(vaultScopeMock.Object);

        vaultMock
            .Setup(v => v.WithScope(It.IsAny<Func<IVaultScope, Task>>()))
            .Returns<Func<IVaultScope, Task>>(async action => await action(vaultScopeMock.Object));

        return vaultMock;
    }
}
