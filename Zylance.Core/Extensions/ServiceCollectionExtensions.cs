using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zylance.Core.Controllers.Services;
using Zylance.Core.Interfaces;
using Zylance.Core.Services;

namespace Zylance.Core.Extensions;

/// <summary>
///     Extension methods for setting up Zylance services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds core Zylance services to the specified <see cref="IServiceCollection" />.
    ///     This includes Gateway, all controllers, and core services.
    ///     Note: Platform-specific implementations (ITransport, IFileProvider, IVaultProvider) must be registered first.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    internal static IServiceCollection AddZylance(this IServiceCollection services)
    {
        services.TryAddSingleton<FileService>();
        services.TryAddSingleton<VaultService>();
        services.AddZylanceRouter();

        services.TryAddSingleton<Gateway>(sp =>
        {
            var transport = sp.GetRequiredService<ITransport>();
            var router = sp.GetRequiredService<RouterService>();
            return new Gateway(transport, router);
        });

        return services;
    }
}
