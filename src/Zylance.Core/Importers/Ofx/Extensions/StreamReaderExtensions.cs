namespace Zylance.Core.Importers.Ofx.Extensions;

/// <summary>
///     Extension methods for StreamReader to support trimmed line reading.
/// </summary>
internal static class StreamReaderExtensions
{
    /// <param name="reader">The StreamReader to read from</param>
    extension(StreamReader reader)
    {
        /// <summary>
        ///     Reads a line from the stream and trims leading and trailing whitespace.
        ///     This ensures consistent handling of indented OFX files.
        /// </summary>
        /// <returns>A trimmed line, or null if the end of the stream has been reached</returns>
        public string? ReadLineTrimmed()
        {
            var line = reader.ReadLine();
            return line?.Trim();
        }
    }
}
