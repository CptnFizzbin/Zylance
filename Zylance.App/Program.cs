using Photino.NET;
using Photino.NET.Server;

namespace Zylance.App;

public static class Program
{
    private const string WindowTitle = "Zylance";
    private const string DefaultServerUrl = "http://localhost:3000";

    [STAThread]
    private static void Main()
    {
        var appUrl = GetServerUrl();

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle(WindowTitle)
            .SetUseOsDefaultLocation(true)
            .SetUseOsDefaultSize(true)
            .SetResizable(true)
            // Most event handlers can be registered after the
            // PhotinoWindow was instantiated by calling a registration
            // method like the following RegisterWebMessageReceivedHandler.
            // This could be added in the PhotinoWindowOptions if preferred.
            .RegisterWebMessageReceivedHandler((sender, message) =>
            {
                if (sender is null) return;

                var window = (PhotinoWindow)sender;

                // The message argument is coming in from sendMessage.
                // "window.external.sendMessage(message: string)"
                var response = $"Received message: \"{message}\"";

                // Send a message back the to JavaScript event handler.
                // "window.external.receiveMessage(callback: Function)"
                window.SendWebMessage(response);
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
