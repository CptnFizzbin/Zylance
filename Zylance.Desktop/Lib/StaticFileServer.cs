using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Zylance.Desktop.Lib;

/// <summary>
///     A simple HTTP server for serving static files from a directory using
///     ASP.NET Core.
/// </summary>
public sealed class StaticFileServer : IAsyncDisposable
{
    private readonly WebApplication _app;
    public string? WebSocketUrl;

    public StaticFileServer(string rootPath, int port)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Root path does not exist: {rootPath}");

        var baseUrl = $"http://localhost:{port}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = rootPath });
        builder.WebHost.UseUrls(baseUrl);

        _app = builder.Build();

        var fileProvider = new PhysicalFileProvider(rootPath);

        _app.UseDefaultFiles();
        _app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = "" });
        _app.MapGet(
            "/ws",
            async context =>
            {
                if (WebSocketUrl == null)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var json = JsonSerializer.Serialize(new { url = WebSocketUrl });
                var bytes = Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";
                await context.Response.Body.WriteAsync(bytes);
            }
        );

        Console.WriteLine($"Static file server configured at {baseUrl}");
        Console.WriteLine($"Serving files from: {rootPath}");
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    public Task StartAsync()
    {
        return _app.RunAsync();
    }
}
