namespace Zylance.Desktop.Tests.TestUtils.Handlers;

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     creation.
/// </summary>
/// <returns>The absolute path to the file to create</returns>
public delegate Task<string> CreateFileHandler(
    string? title,
    string? defaultPath,
    (string Name, string[] Extensions)[]? filters
);
