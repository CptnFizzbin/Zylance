using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Zylance.Desktop.Lib;

/// <summary>
///     A simple HTTP server for serving static files from a directory using ASP.NET Core.
/// </summary>
public sealed class StaticFileServer : IDisposable
{
    private readonly WebApplication _app;

    public StaticFileServer(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Root path does not exist: {rootPath}");

        var port = DiscoverAvailablePort();
        BaseUrl = $"http://localhost:{port}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = rootPath });
        builder.WebHost.UseUrls(BaseUrl);

        _app = builder.Build();

        var fileProvider = new PhysicalFileProvider(rootPath);

        _app.UseDefaultFiles();
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = "" });

        Console.WriteLine($"Static file server configured at {BaseUrl}");
        Console.WriteLine($"Serving files from: {rootPath}");
    }

    public string BaseUrl { get; }

    public void Dispose()
    {
        _app.DisposeAsync().AsTask().Wait();
    }

    /// <summary>
    ///     Discovers an available port by attempting to bind to it.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of ports to try.</param>
    /// <returns>An available port number.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no available port is found after max attempts.</exception>
    private static int DiscoverAvailablePort(int maxAttempts = 10)
    {
        var random = new Random();

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var port = random.Next(8000, 9000); // Random port in 8000-8999 range

            try
            {
                // Try to bind to the port to verify it's available
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                return port;
            }
            catch (SocketException)
            {
                // Port is in use, try another
                if (attempt == maxAttempts - 1)
                    throw new InvalidOperationException(
                        $"Failed to find an available port after {maxAttempts} attempts"
                    );
            }
        }

        throw new InvalidOperationException("Failed to discover an available port");
    }

    public void Start()
    {
        _ = _app.RunAsync();
    }

    public void Stop()
    {
        _ = _app.StopAsync();
    }
}
