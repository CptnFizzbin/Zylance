using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zylance.Core.Attributes;
using Zylance.Core.Interfaces;
using Zylance.Core.Services;

namespace Zylance.Core.Extensions;

/// <summary>
///     Extension methods for setting up Zylance services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Controllers are marked with [RequestController] and preserved")]
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050",
        Justification = "Controllers are marked with [RequestController] and preserved")]
    private static List<Type> GetControllerTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttribute<RequestControllerAttribute>() != null)
            .ToList();
    }

    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds core Zylance services to the specified <see cref="IServiceCollection" />.
        ///     This includes Gateway, all controllers, and core services.
        ///     Note: Platform-specific implementations (ITransport, IFileProvider, IVaultProvider) must be registered first.
        /// </summary>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        [RequiresUnreferencedCode("Controllers are dynamically discovered using reflection")]
        internal IServiceCollection AddZylance()
        {
            services.TryAddSingleton<FileService>();
            services.TryAddSingleton<VaultService>();

            var controllerTypes = GetControllerTypes();
            Console.WriteLine($"[ServiceCollection] Found {controllerTypes.Count} controller types");

            foreach (var controllerType in controllerTypes)
            {
                Console.WriteLine($"[ServiceCollection] Registering controller: {controllerType.Name}");
                services.Add(ServiceDescriptor.Singleton(controllerType, controllerType));
            }

            services.TryAddSingleton<RequestRouter>(sp =>
            {
                Console.WriteLine("[ServiceCollection] Creating RequestRouter");
                var router = new RequestRouter();

                foreach (var controllerType in controllerTypes)
                {
                    Console.WriteLine($"[ServiceCollection] Calling UseController for {controllerType.Name}");
                    
                    // Resolve the controller instance
                    var controller = sp.GetRequiredService(controllerType);
                    
                    // Call UseController<TController>(controller) using reflection
                    var useControllerMethod = typeof(RequestRouter)
                        .GetMethod(nameof(RequestRouter.UseController))!
                        .MakeGenericMethod(controllerType);
                    
                    useControllerMethod.Invoke(router, new[] { controller });
                }

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
