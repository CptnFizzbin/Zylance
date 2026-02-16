using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zylance.Core.Gateway.Services;
using Zylance.Core.Lib.Gateway.Extensions;
using Zylance.Core.Logging;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Context;
using Zylance.Core.Vault.Interfaces;
using Zylance.Core.Vault.Services;

namespace Zylance.Core;

/// <summary>
///     Main application class that coordinates the Gateway and controllers.
///     Manages dependency injection internally for a clean API surface.
/// </summary>
public class ZylanceCore
{
    private static readonly ILogger Log = ZyLogger.ForContext<ZylanceCore>();

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
        Log.Information("Initializing...");

        var services = new ServiceCollection();

        services.AddSingleton(this);

        services.AddSingleton(transport);
        services.AddSingleton(fileProvider);
        services.AddSingleton(vaultProvider);

        services.AddSingleton<FileService>();
        services.AddSingleton<VaultService>();
        services.AddSingleton<VaultContext>();
        services.AddSingleton<BackgroundTaskService>();
        services.AddZylanceRouter();
        services.AddSingleton<GatewayService>();

        Log.Information("Building service provider...");
        IServiceProvider serviceProvider = services.BuildServiceProvider();

        // Resolve and cache the vault context
        Log.Information("Initializing Gateway...");
        Gateway = serviceProvider.GetRequiredService<GatewayService>();

        Log.Information("Initialization complete!");
    }

    /// <summary>
    ///     Gets the initialized GatewayService instance.
    /// </summary>
    public GatewayService Gateway { get; }
}
