using Photino.NET;
using Photino.NET.Server;
using Zylance.Core;
using Zylance.Gateway;

namespace Zylance.Desktop;

public static class Program
{
    private const string WindowTitle = "Zylance";
    private const string DefaultUiServerUrl = "http://localhost:3000";

    [STAThread]
    private static void Main()
    {
        var appUrl = GetServerUrl();

        var window = new PhotinoWindow()
            .SetTitle(WindowTitle)
            .SetUseOsDefaultLocation(true)
            .SetUseOsDefaultSize(true)
            .SetResizable(true)
            .SetDevToolsEnabled(DevToolsEnabled())
            .Load(appUrl);

        ZylanceCore.Listen(
            new ZylanceGateway(
                new PhotinoTransport(window),
                new DesktopFileProvider(window)
            )
        );

        Console.WriteLine($"Starting {WindowTitle} application...");
        window.WaitForClose();
    }

    private static bool DevToolsEnabled()
    {
        return Environment.GetEnvironmentVariable("ZYLANCE_DEVTOOLS") == "true";
    }

    private static string GetUiMode()
    {
        return Environment.GetEnvironmentVariable("ZYLANCE_UI_MODE") ?? "internal";
    }

    private static Uri GetServerUrl()
    {
        var serverUrl = GetUiMode() == "external"
            ? GetZylanceUiUrl()
            : StartWebServer();

        return new Uri(serverUrl);
    }

    private static string GetZylanceUiUrl()
    {
        var debugServer = Environment.GetEnvironmentVariable("ZYLANCE_UI_URL");
        return string.IsNullOrWhiteSpace(debugServer)
            ? DefaultUiServerUrl
            : debugServer;
    }

    private static string StartWebServer()
    {
        var args = Environment.GetCommandLineArgs();
        PhotinoServer
            .CreateStaticFileServer(args, out var serverUrl)
            .RunAsync();

        return serverUrl;
    }
}
