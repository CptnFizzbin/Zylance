using Moq;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Interfaces;

namespace Zylance.Core.Tests.TestUtils.Factories;

public static class VaultContextTestFactory
{
    public static VaultContext Create(
        Mock<IVault>? vaultMock = null,
        Mock<ITransport>? transportMock = null,
        Mock<GatewayService>? gatewayMock = null
    )
    {
        var transport = transportMock?.Object ?? new Mock<ITransport>().Object;
        var gateway = gatewayMock?.Object ?? new Mock<GatewayService>(transport).Object;
        var context = new VaultContext(gateway);
        if (vaultMock != null)
            context.OpenVault(vaultMock.Object);
        return context;
    }
}
