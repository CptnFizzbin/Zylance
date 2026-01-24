using Microsoft.Extensions.DependencyInjection;
using Zylance.Core.Extensions;
using Zylance.Core.Interfaces;

namespace Zylance.Core;

/// <summary>
///     Main application class that coordinates the Gateway and controllers.
///     Manages dependency injection internally for a clean API surface.
/// </summary>
public class Zylance
{
    private readonly IServiceProvider _serviceProvider;
    
    public Gateway Gateway { get; }

    /// <summary>
    ///     Initializes a new instance of Zylance with platform-specific implementations.
    ///     The DI container is managed internally.
    /// </summary>
    /// <param name="transport">The transport implementation for communication.</param>
    /// <param name="fileProvider">The file provider implementation.</param>
    /// <param name="vaultProvider">The vault provider implementation.</param>
    public Zylance(
        ITransport transport,
        IFileProvider fileProvider,
        IVaultProvider vaultProvider
    )
    {
        Console.WriteLine("[Zylance] Initializing...");
        
        // Build the internal DI container
        var services = new ServiceCollection();

        // Register platform-specific implementations
        services.AddSingleton(transport);
        services.AddSingleton(fileProvider);
        services.AddSingleton(vaultProvider);

        // Register all core Zylance services
        Console.WriteLine("[Zylance] Calling AddZylance()...");
        services.AddZylance();

        Console.WriteLine("[Zylance] Building service provider...");
        _serviceProvider = services.BuildServiceProvider();

        // Resolve and cache the gateway
        Console.WriteLine("[Zylance] Resolving Gateway...");
        Gateway = _serviceProvider.GetRequiredService<Gateway>();
        Console.WriteLine("[Zylance] Initialization complete!");
    }
}
