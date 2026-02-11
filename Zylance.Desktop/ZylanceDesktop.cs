using Photino.NET;
using Zylance.Desktop.Config;
using Zylance.Desktop.Lib;
using Zylance.Desktop.Providers;
using Zylance.Desktop.Transports;

namespace Zylance.Desktop;

public class ZylanceDesktop(ZylanceDesktopConfig config) : IAsyncDisposable
{
    private StaticFileServer? _webServer;
    private PhotinoWindow? _window;

    public async ValueTask DisposeAsync()
    {
        if (_webServer != null)
            await _webServer.DisposeAsync();

        _window?.Close();

        GC.SuppressFinalize(this);
    }

    public ZylanceDesktop Start()
    {
        if (config.UiServerEnabled)
            _webServer = StartWebServer();

        _window = CreateWindow();

        var transport = new PhotinoTransport(_window);
        var fileProvider = new DesktopFileProvider(_window, config.AppDataPath, config.TmpDataPath);
        var vaultProvider = new DesktopVaultProvider(fileProvider);

        _ = new Core.Zylance(transport, fileProvider, vaultProvider);

        return this;
    }

    public void WaitForExit()
    {
        _window?.WaitForClose();
    }

    private PhotinoWindow CreateWindow()
    {
        return new PhotinoWindow()
            .SetTitle(ZylanceDesktopConfig.AppName)
            .SetUseOsDefaultLocation(true)
            .SetUseOsDefaultSize(true)
            .SetResizable(true)
            .SetDevToolsEnabled(config.DevToolsEnabled)
            .Load(config.UiServerUrl);
    }

    private StaticFileServer StartWebServer()
    {
        Console.WriteLine($"Starting web server on port {config.UiPort}...");
        var wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var server = new StaticFileServer(wwwrootPath, config.UiPort);
        server.StartAsync();
        return server;
    }
}
