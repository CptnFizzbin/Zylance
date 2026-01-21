using Photino.NET;
using Photino.NET.Server;
using Zylance.App.Extensions;

namespace Zylance.App;

public static class Program
{
    private const string WindowTitle = "Zylance";
    private const string DefaultServerUrl = "http://localhost:3000";

    [STAThread]
    private static void Main()
    {
        var appUrl = GetServerUrl();
        var appBridge = new AppBridge();

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle(WindowTitle)
            .SetUseOsDefaultLocation(true)
            .SetUseOsDefaultSize(true)
            .SetResizable(true)
            .SetDevToolsEnabled(IsDebugMode())
            .RegisterJsonMessageHandler(AppBridge.HandleJsonMessage)
            .RegisterWebMessageReceivedHandler((sender, message) =>
            {
                Console.WriteLine($"Received message: {message}");
                var window = (PhotinoWindow)sender!;
                window.SendWebMessage($"Echo: {message}");
            })
            // Can be used with relative path strings or "new URI()" instance to load a website.
            .Load(appUrl);

        Console.WriteLine($"Starting {WindowTitle} application...");
        window.WaitForClose();
    }

    private static bool IsDebugMode()
    {
        var args = Environment.GetCommandLineArgs();
        var debugEnv = Environment.GetEnvironmentVariable("ZYLANCE_DEBUG")?.ToLower();

        return args.Contains("--debug")
            || args.Contains("-d")
            || debugEnv == "1"
            || debugEnv == "true";
    }

    private static Uri GetServerUrl()
    {
        var serverUrl = IsDebugMode()
            ? GetDebugServerUrl()
            : StartWebServer();

        return new Uri(serverUrl);
    }

    private static string GetDebugServerUrl()
    {
        var debugServer = Environment.GetEnvironmentVariable("ZYLANCE_SERVER");
        return string.IsNullOrWhiteSpace(debugServer)
            ? DefaultServerUrl
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
