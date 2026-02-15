using Serilog;
using Zylance.Desktop.Config;

namespace Zylance.Desktop;

/// <summary>
///     Entry point for the Zylance Desktop application.
/// </summary>
public static class Program
{
    [STAThread]
    private static void Main()
    {
        var config = new ZylanceDesktopConfig();
        var sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var logDir = config.LogPath;
        Directory.CreateDirectory(logDir);
        var logPath = Path.Combine(logDir, $"zylance-{sessionId}.log");

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logPath, shared: true)
            .CreateLogger();

        RotateLogs(logPath);

        try
        {
            Log.Information("Starting Zylance Desktop");
            new ZylanceDesktop(config).Start().WaitForExit();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void RotateLogs(string logDir)
    {
        var filesToDelete = new DirectoryInfo(logDir)
            .GetFiles("zylance-*.log")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Skip(10);

        foreach (var f in filesToDelete)
            f.Delete();
    }
}
