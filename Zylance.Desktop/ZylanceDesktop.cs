using Photino.NET;
using Zylance.Desktop.Config;
using Zylance.Desktop.Lib;
using Zylance.Desktop.Providers;
using Zylance.Desktop.Transports;

namespace Zylance.Desktop;

public class ZylanceDesktop : IAsyncDisposable
{
    private readonly ZylanceDesktopConfig _config;
    private ZylanceInternalServer? _webServer;
    private PhotinoWindow? _window;

    public ZylanceDesktop(ZylanceDesktopConfig config)
    {
        _config = config;
    }

    public async ValueTask DisposeAsync()
    {
        if (_webServer != null)
            await _webServer.DisposeAsync();

        _window?.Close();

        GC.SuppressFinalize(this);
    }

    public ZylanceDesktop Start()
    {
        if (_config.UiServerEnabled)
            _webServer = StartWebServer();

        _window = CreateWindow();

        var transport = new WebsocketTransport(_config.WsPort);
        var fileProvider = new DesktopFileProvider(_window, _config.AppDataPath, _config.TmpDataPath);
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
            .SetDevToolsEnabled(_config.DevToolsEnabled)
            .Load(_config.UiServerUrl);
    }

    private ZylanceInternalServer StartWebServer()
    {
        Console.WriteLine($"Starting web server on port {_config.UiPort}...");
        var server = new ZylanceInternalServer(_config);
        server.StartAsync();
        return server;
    }
}
