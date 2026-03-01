using System.Text;

namespace Zylance.Core.Tests.TestUtils.Fixtures;

public static class FixtureUtils
{
    /// <summary>
    ///     Loads a fixture file from the Fixtures directory
    /// </summary>
    /// <param name="relativePath">
    ///     Path relative to Fixtures/ directory (e.g.,
    ///     "Importers/Ofx/V1/example.ofx")
    /// </param>
    public static StreamReader LoadFixture(string relativePath)
    {
        var baseDir = Directory.GetCurrentDirectory();

        var fixtureDir = Path.Combine(baseDir, "TestUtils", "Fixtures");
        var filePath = Path.Combine(fixtureDir, relativePath);

        return !File.Exists(filePath)
            ? throw new FileNotFoundException($"Fixture file not found: {filePath}")
            : new StreamReader(File.OpenRead(filePath));
    }

    /// <summary>
    ///     Converts a string containing OFX content to a StreamReader.
    ///     Allows tests to provide indented or formatted OFX content; downstream
    ///     extension methods trim lines so parsing stays consistent regardless of
    ///     whitespace.
    /// </summary>
    public static StreamReader StringToStreamReader(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new StreamReader(stream);
    }
}
