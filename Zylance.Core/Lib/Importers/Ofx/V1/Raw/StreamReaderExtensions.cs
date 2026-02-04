namespace Zylance.Core.Lib.Importers.Ofx.V1.Raw;

/// <summary>
/// Extension methods for StreamReader to support trimmed line reading.
/// </summary>
internal static class StreamReaderExtensions
{
    /// <summary>
    /// Reads a line from the stream and trims leading and trailing whitespace.
    /// This ensures consistent handling of indented OFX files.
    /// </summary>
    /// <param name="reader">The StreamReader to read from</param>
    /// <returns>A trimmed line, or null if the end of the stream has been reached</returns>
    public static string? ReadLineTrimmed(this StreamReader reader)
    {
        var line = reader.ReadLine();
        return line?.Trim();
    }

    /// <summary>
    /// Asynchronously reads a line from the stream and trims leading and trailing whitespace.
    /// This ensures consistent handling of indented OFX files.
    /// </summary>
    /// <param name="reader">The StreamReader to read from</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A task representing the asynchronous operation with a trimmed line, or null if the end of the stream has been reached</returns>
    public static async ValueTask<string?> ReadLineTrimmedAsync(this StreamReader reader, CancellationToken cancellationToken)
    {
        var line = await reader.ReadLineAsync(cancellationToken);
        return line?.Trim();
    }
}
