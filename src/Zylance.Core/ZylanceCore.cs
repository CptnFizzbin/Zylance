using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zylance.Core.App.Services;
using Zylance.Core.Lib;
using Zylance.Core.Lib.Gateway;
using Zylance.Core.Lib.Gateway.Extensions;
using Zylance.Core.Lib.Gateway.Services;
using Zylance.Core.Lib.Vault;

namespace Zylance.Core;

/// <summary>
///     Main application class that coordinates the Gateway and controllers.
///     Manages dependency injection internally for a clean API surface.
/// </summary>
public class ZylanceCore
{
    /// <summary>
    ///     Initializes a new instance of Zylance with platform-specific
    ///     implementations.
    ///     The DI container is managed internally.
    /// </summary>
    /// <param name="transport">The transport implementation for communication.</param>
    /// <param name="fileProvider">The file provider implementation.</param>
    /// <param name="vaultProvider">The vault provider implementation.</param>
    public ZylanceCore(ITransport transport, IFileProvider fileProvider, IVaultProvider vaultProvider)
    {
        Console.WriteLine("[Zylance] Initializing...");

        // Build the internal DI container
        var services = new ServiceCollection();

        services.AddSingleton(this);

        // Register platform-specific implementations
        services.AddSingleton(transport);
        services.AddSingleton(fileProvider);
        services.AddSingleton(vaultProvider);

        // Register all core Zylance services
        Console.WriteLine("[Zylance] Calling AddZylance()...");
        services.AddSingleton<FileService>();
        services.AddSingleton<VaultService>();
        services.AddSingleton<VaultContext>();
        services.AddSingleton<BackgroundTaskService>();
        services.AddZylanceRouter();

        services.TryAddSingleton<GatewayService>(sp =>
        {
            var router = sp.GetRequiredService<RouterService>();
            return new GatewayService(transport, router);
        });

        Console.WriteLine("[Zylance] Building service provider...");
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Resolve and cache the vault context
        Console.WriteLine("[Zylance] Initializing Gateway...");
        Gateway = serviceProvider.GetRequiredService<GatewayService>();

        Console.WriteLine("[Zylance] Initialization complete!");
    }

    /// <summary>
    /// Gets the initialized GatewayService instance.
    /// </summary>
    public GatewayService Gateway { get; }
}
