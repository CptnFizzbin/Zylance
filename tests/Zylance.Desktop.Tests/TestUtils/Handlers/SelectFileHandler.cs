namespace Zylance.Desktop.Tests.TestUtils.Handlers;

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     selection.
/// </summary>
/// <returns>The absolute path to the file to select</returns>
public delegate Task<string> SelectFileHandler(
    string? title,
    (string Name, string[] Extensions)[]? filters,
    bool readOnly
);
