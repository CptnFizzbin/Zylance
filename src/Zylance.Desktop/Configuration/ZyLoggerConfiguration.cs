using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Zylance.Desktop.Configuration;

/// <summary>
///     Provides configuration and creation of the Serilog logger for the
///     Zylance.Desktop application.
///     Handles log file rotation and output destinations.
/// </summary>
public static class ZyLoggerConfiguration
{
    /// <summary>
    ///     Creates and configures a Serilog <see cref="ILogger" /> instance for the
    ///     desktop application.
    /// </summary>
    /// <param name="config">The application configuration containing the log path.</param>
    /// <returns>A configured Serilog <see cref="ILogger" /> instance.</returns>
    public static ILogger CreateLogger(ZyConfiguration config)
    {
        var sessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        Directory.CreateDirectory(config.LogPath);
        var logPath = Path.Combine(config.LogPath, $"zylance-{sessionId}.log");
        var errorLogPath = Path.Combine(config.LogPath, $"zylance-{sessionId}.error.log");

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(new CompactJsonFormatter(), logPath, LogEventLevel.Information, shared: true)
            .WriteTo.File(new CompactJsonFormatter(), errorLogPath, LogEventLevel.Error, shared: true)
            .CreateLogger();

        RotateLogs(config.LogPath);

        return logger;
    }

    /// <summary>
    ///     Rotates log files in the specified directory, keeping only the 10 most
    ///     recent logs.
    /// </summary>
    /// <param name="logDir">The directory containing log files.</param>
    private static void RotateLogs(string logDir)
    {
        try
        {
            var logFiles = new DirectoryInfo(logDir)
                .GetFiles("zylance-*.log")
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            if (logFiles.Count <= 10)
                return;

            var filesToDelete = logFiles.Skip(10);
            foreach (var f in filesToDelete)
                f.Delete();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to rotate logs");
        }
    }
}
