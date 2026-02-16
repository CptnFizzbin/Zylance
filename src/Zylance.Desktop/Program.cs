using Serilog;
using Zylance.Desktop.Configuration;

namespace Zylance.Desktop;

/// <summary>
///     Entry point for the Zylance Desktop application.
/// </summary>
public static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            var config = new ZyConfiguration();
            Log.Logger = ZyLoggerConfiguration.CreateLogger(config);

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
}
