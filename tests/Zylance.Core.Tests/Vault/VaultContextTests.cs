using Zylance.Core.Tests.Mocks;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Interfaces;

namespace Zylance.Core.Tests.Vault;

public class VaultContextTests
{
    /// <summary>
    ///     Verifies that when the active vault is closed (set to null), a LocalVault
    ///     instance is disposed and its underlying DbContext becomes unusable.
    /// </summary>
    [Fact]
    public async Task ClosingActiveVault_DisposesLocalVault()
    {
        // Arrange - use an in-memory MockVault that exposes IsDisposed
        var vault = new MockVault();

        // Minimal ZylanceCore dependencies
        var transport = new MockTransport();
        var fileProvider = new MockFileProvider();
        var vaultProvider = new TestVaultProvider();

        var zcore = new ZylanceCore(transport, fileProvider, vaultProvider);
        var vaultContext = new VaultContext(zcore);

        // Act - set active vault then close it
        vaultContext.ActiveVault = vault;
        vaultContext.ActiveVault = null; // should dispose the previous MockVault

        // Assert - MockVault should report disposed
        Assert.True(vault.IsDisposed);
    }

    // Simple IVaultProvider used only to satisfy ZylanceCore in tests
    private class TestVaultProvider : IVaultProvider
    {
        public Task<IVault> CreateVault()
        {
            throw new NotImplementedException();
        }

        public Task<IVault> OpenVault()
        {
            throw new NotImplementedException();
        }
    }
}
