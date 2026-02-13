using Photino.NET;
using Zylance.Contract;
using Zylance.Core;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Vault.Interfaces;
using Zylance.Desktop.Config;
using Zylance.Desktop.Providers;
using Zylance.Desktop.Services;
using Zylance.Desktop.Transports;

namespace Zylance.Desktop;

public class ZylanceDesktop(
    ZylanceDesktopConfig config,
    ITransport? transport = null,
    ILocalFileProvider? fileProvider = null,
    IVaultProvider? vaultProvider = null
) : IAsyncDisposable
{
    private ILocalFileProvider? _fileProvider = fileProvider;
    private ITransport? _transport = transport;
    private IVaultProvider? _vaultProvider = vaultProvider;
    private WebServerService? _webServer;
    private PhotinoWindow? _window;
    private ZylanceCore? _zylanceCore;

    public bool IsDisposed { get; private set; }

    public ZylanceCore ZylanceCore =>
        _zylanceCore
        ?? throw new InvalidOperationException(
            "ZylanceDesktop has not been started. Call Start() before accessing ZylanceCore."
        );

    public ZylanceDesktopConfig Config => config;

    public async ValueTask DisposeAsync()
    {
        if (_webServer is not null)
            await _webServer.DisposeAsync();

        _window?.Close();
        IsDisposed = true;

        GC.SuppressFinalize(this);
    }

    public ZylanceDesktop Start()
    {
        if (config.UiServerEnabled)
            _webServer = StartWebServer();

        if (!config.Headless)
        {
            _window = CreateWindow();
            _fileProvider ??= new DesktopFileProvider(_window, config.AppDataPath, config.TmpDataPath);
        }
        else
        {
            if (_fileProvider == null)
                throw new InvalidOperationException(
                    "A file provider must be provided in headless mode. Please provide an implementation of ILocalFileProvider when constructing ZylanceDesktop in headless mode."
                );
        }

        _transport ??= new WebsocketTransport(config.WsPort);
        _vaultProvider ??= new DesktopVaultProvider(_fileProvider);

        _zylanceCore = new ZylanceCore(_transport, _fileProvider, _vaultProvider);
        _zylanceCore.Gateway.ObserveEvent(ZylanceConstants.Events.Desktop_Exit).Subscribe(_ => Exit());

        return this;
    }

    private void Exit()
    {
        Console.WriteLine("Exit requested. Closing application...");
        DisposeAsync().AsTask().Wait();
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

    private WebServerService StartWebServer()
    {
        Console.WriteLine($"Starting web server on port {config.UiPort}...");
        var server = new WebServerService(config);
        server.StartAsync();
        return server;
    }
}
