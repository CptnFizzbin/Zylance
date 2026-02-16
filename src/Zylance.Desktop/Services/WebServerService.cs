using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Zylance.Desktop.Configuration;
using static Zylance.Core.Logging.ZyLogger;

namespace Zylance.Desktop.Services;

/// <summary>
///     A simple HTTP server for serving static files from a directory using
///     ASP.NET Core.
/// </summary>
public sealed class WebServerService : IAsyncDisposable
{
    private static readonly ILogger Log = ForContext<WebServerService>();
    private readonly WebApplication _app;

    /// <summary>
    ///     Initializes a new instance of <see cref="WebServerService" /> using the
    ///     provided configuration.
    /// </summary>
    /// <param name="config">
    ///     Desktop configuration containing UI server and webroot
    ///     settings.
    /// </param>
    public WebServerService(ZyConfiguration config)
    {
        Log.Debug("Initializing WebServerService with UiServerUrl={Url}", config.UiServerUrl);

        if (!Directory.Exists(config.UiRootPath))
            throw new DirectoryNotFoundException($"Root path does not exist: {config.UiRootPath}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = config.UiRootPath });
        builder.WebHost.UseUrls(config.UiServerUrl);

        _app = builder.Build();

        var fileProvider = new PhysicalFileProvider(config.UiRootPath);

        _app.Use(
            async (context, next) =>
            {
                Log.Debug(
                    "Incoming request {Method} {Path}",
                    Sanitize(context.Request.Method),
                    Sanitize(context.Request.Path.ToString())
                );
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

        Log.Information("Static file server configured at {ConfigUiServerUrl}", config.UiServerUrl);
        Log.Information("Serving files from: {ConfigUiRootPath}", config.UiRootPath);
    }

    /// <summary>
    ///     Disposes the internal web application and associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        Log.Debug("Disposing WebServerService");
        await _app.DisposeAsync();
    }

    /// <summary>
    ///     Starts the internal static file web server and begins listening for
    ///     requests.
    /// </summary>
    public Task StartAsync()
    {
        Log.Information("Starting WebServerService");
        return _app.RunAsync();
    }
}
