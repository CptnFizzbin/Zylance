using static Zylance.Desktop.Utils.WebUtils;

namespace Zylance.Desktop.Config;

/// <summary>
/// Configuration for the Zylance desktop application.
/// </summary>
public record ZylanceDesktopConfig
{
    /// <summary>
    /// Creates a new configuration, optionally overriding the UI and WebSocket ports.
    /// </summary>
    public ZylanceDesktopConfig(int? uiPort = null, int? wsPort = null)
    {
        UiPort = uiPort ?? GetInt("UI_PORT") ?? DiscoverAvailablePort(8000, 8999);
        WsPort = wsPort ?? GetInt("WS_PORT") ?? DiscoverAvailablePort(9000, 9999);
    }

    /// <summary>The application name shown in the window title and data paths.</summary>
    public const string AppName = "Zylance";

    /// <summary>When true, run without a native window (headless mode).</summary>
    public bool Headless { get; init; }

    /// <summary>Whether the built-in UI server should be started.</summary>
    public bool UiServerEnabled { get; init; } = GetBool("UI_SERVER_ENABLED") ?? true;

    /// <summary>The port used by the UI server.</summary>
    public int UiPort { get; init; }

    /// <summary>The port used by the WebSocket transport.</summary>
    public int WsPort { get; init; }

    /// <summary>Enable developer tools in the native window.</summary>
    public bool DevToolsEnabled { get; init; } = GetBool("DEVTOOLS_ENABLED") ?? false;

    /// <summary>URL to the local UI server.</summary>
    public string UiServerUrl => $"http://localhost:{UiPort}";

    /// <summary>WebSocket URL used by the transport.</summary>
    public string WebSocketUrl => $"ws://localhost:{WsPort}";

    /// <summary>Filesystem path to the UI root (wwwroot).</summary>
    public string UiRootPath { get; init; } = Path.Combine(AppContext.BaseDirectory, "wwwroot");

    /// <summary>Path where application data is stored for the current user.</summary>
    public string AppDataPath { get; init; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    /// <summary>Temporary data path used by the application.</summary>
    public string TmpDataPath { get; init; } = Path.Combine(Path.GetTempPath(), AppName, Guid.NewGuid().ToString());

    /// <summary>Path where log files should be stored for the current user.</summary>
    public string LogPath { get; init; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName, "logs");

    private static string? GetFlagValue(string name)
    {
        var args = Environment.GetCommandLineArgs();
        var flagName = name.ToLowerInvariant().Replace('_', '-');

        return (
            from arg in args
            where arg.StartsWith("--" + flagName + "=") || arg.StartsWith("/" + flagName + "=")
            let idx = arg.IndexOf('=')
            where idx > 0
            select arg[(idx + 1)..]
        ).FirstOrDefault();
    }

    private static string? GetEnvValue(string name)
    {
        var envVarName = $"ZYLANCE_{name.ToUpperInvariant()}";
        return Environment.GetEnvironmentVariable(envVarName);
    }

    private static string? GetString(string name)
    {
        return GetFlagValue(name) ?? GetEnvValue(name);
    }

    private static bool? GetBool(string name)
    {
        return bool.TryParse(GetString(name), out var value) ? value : null;
    }

    private static int? GetInt(string name)
    {
        return int.TryParse(GetString(name), out var value) ? value : null;
    }
}
