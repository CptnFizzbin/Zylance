namespace Zylance.Desktop.Tests.TestUtils.Fixtures;

public static class FixtureUtils
{
    /// <summary>
    ///     Fetches the absolute path to a fixture file given its relative path from
    ///     the Fixtures/ directory.
    /// </summary>
    /// <param name="relativePath">
    ///     Path relative to Fixtures/ directory (e.g., "Importers/Ofx/V1/example.ofx")
    /// </param>
    public static string GetFixturePath(string relativePath)
    {
        var baseDir = Directory.GetCurrentDirectory();
        var fixtureDir = Path.Combine(baseDir, "TestUtils", "Fixtures");
        return Path.Combine(fixtureDir, relativePath);
    }
}
