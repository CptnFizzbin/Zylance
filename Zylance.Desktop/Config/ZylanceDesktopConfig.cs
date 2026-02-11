using static Zylance.Desktop.Lib.WebUtils;

namespace Zylance.Desktop.Config;

public record ZylanceDesktopConfig
{
    public const string AppName = "Zylance";

    public bool UiServerEnabled { get; init; } = GetBool("UI_SERVER_ENABLED") ?? true;
    public int UiPort { get; init; } = GetInt("UI_PORT") ?? DiscoverAvailablePort(8000, 8099);
    public int WsPort { get; init; } = GetInt("WS_PORT") ?? DiscoverAvailablePort(8100, 8199);
    public bool DevToolsEnabled { get; init; } = GetBool("DEVTOOLS_ENABLED") ?? false;

    public string UiServerUrl => $"http://localhost:{UiPort}";
    public string WebSocketUrl => $"ws://localhost:{WsPort}";

    public string AppDataPath { get; init; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    public string TmpDataPath { get; init; } = Path.Combine(Path.GetTempPath(), AppName, Guid.NewGuid().ToString());

    private static string? GetFlagString(string envVarName)
    {
        var args = Environment.GetCommandLineArgs();
        var flagName = envVarName.ToLowerInvariant().Replace('_', '-');

        return (
            from arg in args
            where arg.StartsWith("--" + flagName + "=") || arg.StartsWith("/" + flagName + "=")
            let idx = arg.IndexOf('=')
            where idx > 0
            select arg[(idx + 1)..]
        ).FirstOrDefault();
    }

    private static string? GetString(string name)
    {
        return GetFlagString(name) ?? Environment.GetEnvironmentVariable(name);
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
