using Photino.NET;
using Zylance.Desktop.Config;
using Zylance.Desktop.Lib;

namespace Zylance.Desktop;

public static class Program
{
    private const string WindowTitle = "Zylance";

    [STAThread]
    private static void Main()
    {
        var config = new ZylanceDesktopConfig();

        if (config.UiServerEnabled)
            StartWebServer(config.UiPort);

        var window = new PhotinoWindow()
            .SetTitle(ZylanceDesktopConfig.AppName)
            .SetUseOsDefaultLocation(true)
            .SetUseOsDefaultSize(true)
            .SetResizable(true)
            .SetDevToolsEnabled(config.DevToolsEnabled)
            .Load(config.UiServerUrl);

        var transport = new PhotinoTransport(window);
        var fileProvider = new DesktopFileProvider(window, config.AppDataPath, config.TmpDataPath);
        var vaultProvider = new DesktopVaultProvider(fileProvider);

        _ = new Core.Zylance(transport, fileProvider, vaultProvider);

        Console.WriteLine($"Starting {WindowTitle} application...");
        window.WaitForClose();
    }

    private static void StartWebServer(int port)
    {
        var wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var server = new StaticFileServer(wwwrootPath, port);
        server.StartAsync();
    }
}
