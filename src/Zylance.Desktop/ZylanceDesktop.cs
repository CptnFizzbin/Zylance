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

/// <summary>
/// Hosts the Zylance core and (optionally) a native window and UI server.
/// </summary>
/// <param name="config">Configuration for the desktop instance.</param>
/// <param name="transport">Optional transport implementation (defaults to WebSocket).</param>
/// <param name="fileProvider">Optional local file provider for headless mode or custom file handling.</param>
/// <param name="vaultProvider">Optional vault provider for persistent storage.</param>
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

    /// <summary>Whether DisposeAsync has been called.</summary>
    public bool IsDisposed { get; private set; }

    /// <summary>Access to the running ZylanceCore instance. Throws if Start() has not been called.</summary>
    public ZylanceCore ZylanceCore =>
        _zylanceCore
        ?? throw new InvalidOperationException(
            "ZylanceDesktop has not been started. Call Start() before accessing ZylanceCore."
        );

    /// <summary>The configuration used to construct this desktop instance.</summary>
    public ZylanceDesktopConfig Config => config;

    /// <summary>Dispose resources used by the desktop and stop the webserver and window.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_webServer is not null)
            await _webServer.DisposeAsync();

        _window?.Close();
        IsDisposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>Starts the desktop: web server, window (unless headless), transport and core.</summary>
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

    /// <summary>Block until the native window is closed (no-op in headless mode).</summary>
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
