using Zylance.Desktop.Config;

namespace Zylance.Desktop;

/// <summary>
/// Entry point for the Zylance Desktop application.
/// </summary>
public static class Program
{
    [STAThread]
    private static void Main()
    {
        var config = new ZylanceDesktopConfig();
        new ZylanceDesktop(config).Start().WaitForExit();
    }
}
