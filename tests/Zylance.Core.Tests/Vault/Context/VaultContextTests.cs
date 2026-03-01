using Moq;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Vault.Context;

namespace Zylance.Core.Tests.Vault.Context;

public class VaultContextTests
{
    /// <summary>
    ///     Verifies that when the active vault is closed (set to null), a LocalVault
    ///     instance is disposed and its underlying DbContext becomes unusable.
    /// </summary>
    [Fact]
    public void ClosingActiveVault_DisposesLocalVault()
    {
        // Arrange
        var vaultMock = VaultTestFactory.Create();
        vaultMock.Setup(v => v.DisposeAsync()).Verifiable();

        var transportMock = new Mock<ITransport>();
        var gateway = new GatewayService(transportMock.Object);
        var vaultContext = new VaultContext(gateway);
        vaultContext.OpenVault(vaultMock.Object);

        // Act
        vaultContext.CloseVault();

        // Assert
        vaultMock.Verify(v => v.DisposeAsync(), Times.Once);
    }
}
