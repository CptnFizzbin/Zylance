using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zylance.Core.Controllers;
using Zylance.Core.Interfaces;
using Zylance.Core.Services;

namespace Zylance.Core.Extensions;

/// <summary>
///     Extension methods for setting up Zylance services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds core Zylance services to the specified <see cref="IServiceCollection" />.
        ///     This includes Gateway, all controllers, and core services.
        ///     Note: Platform-specific implementations (ITransport, IFileProvider, IVaultProvider) must be registered first.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        internal IServiceCollection AddZylance()
        {
            // Application services
            services.TryAddSingleton<FileService>();
            services.TryAddSingleton<VaultService>();

            // Controllers
            services.TryAddSingleton<FileController>();
            services.TryAddSingleton<VaultController>();
            services.TryAddSingleton<EchoController>();
            services.TryAddSingleton<StatusController>();

            // Centralized RequestRouter - automatically discovers and registers all controllers
            services.TryAddSingleton<RequestRouter>(sp =>
            {
                var router = new RequestRouter();

                // Auto-register all controllers
                router.UseController(sp.GetRequiredService<FileController>());
                router.UseController(sp.GetRequiredService<VaultController>());
                router.UseController(sp.GetRequiredService<EchoController>());
                router.UseController(sp.GetRequiredService<StatusController>());

                return router;
            });

            // Gateway - uses the centralized RequestRouter
            services.TryAddSingleton<Gateway>(sp =>
            {
                var transport = sp.GetRequiredService<ITransport>();
                var router = sp.GetRequiredService<RequestRouter>();
                return new Gateway(transport, router);
            });

            return services;
        }
    }
}
