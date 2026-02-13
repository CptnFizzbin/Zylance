using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Zylance.Desktop.Config;

namespace Zylance.Desktop.Lib;

/// <summary>
///     A simple HTTP server for serving static files from a directory using
///     ASP.NET Core.
/// </summary>
public sealed class ZylanceInternalServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    /// <summary>
    /// Initializes a new instance of <see cref="ZylanceInternalServer"/> using the provided configuration.
    /// </summary>
    /// <param name="config">Desktop configuration containing UI server and webroot settings.</param>
    public ZylanceInternalServer(ZylanceDesktopConfig config)
    {
        if (!Directory.Exists(config.UiRootPath))
            throw new DirectoryNotFoundException($"Root path does not exist: {config.UiRootPath}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = config.UiRootPath });
        builder.WebHost.UseUrls(config.UiServerUrl);

        _app = builder.Build();

        var fileProvider = new PhysicalFileProvider(config.UiRootPath);

        _app.Use(
            async (context, next) =>
            {
                if (context.Request.Path == "/" || context.Request.Path == "/index.html")
                {
                    var htmlPath = Path.Combine(config.UiRootPath, "index.html");
                    if (File.Exists(htmlPath))
                    {
                        var html = await File.ReadAllTextAsync(htmlPath);
                        var replacedHtml = html.Replace("{{zylance.webSocketUrl}}", config.WebSocketUrl);
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(replacedHtml);
                        return;
                    }
                }

                await next();
            }
        );

        _app.UseDefaultFiles();
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = "" });

        Console.WriteLine($"Static file server configured at {config.UiServerUrl}");
        Console.WriteLine($"Serving files from: {config.UiRootPath}");
    }

    /// <summary>
    /// Disposes the internal web application and associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    /// <summary>
    /// Starts the internal static file web server and begins listening for requests.
    /// </summary>
    public Task StartAsync()
    {
        return _app.RunAsync();
    }
}
