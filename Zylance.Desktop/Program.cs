using Zylance.Desktop.Config;

namespace Zylance.Desktop;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        var config = new ZylanceDesktopConfig();
        new ZylanceDesktop(config).Start().WaitForExit();
    }
}
