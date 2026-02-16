using Serilog;

namespace Zylance.Core.Logging;

/// <summary>
///     Provides a strongly-typed Serilog logger for use throughout the
///     Zylance.Core project.
/// </summary>
public static class ZyLogger
{
    /// <summary>
    ///     Creates a Serilog <see cref="ILogger" /> instance for the specified class
    ///     type.
    /// </summary>
    /// <typeparam name="TClass">The class type for which to create the logger context.</typeparam>
    /// <returns>
    ///     A Serilog <see cref="ILogger" /> configured for the given class
    ///     context.
    /// </returns>
    public static ILogger ForContext<TClass>()
    {
        return Log.ForContext<TClass>();
    }

    /// <summary>
    ///     Creates a Serilog <see cref="ILogger" /> instance for the specified class
    ///     type.
    /// </summary>
    /// <returns>
    ///     A Serilog <see cref="ILogger" /> configured for the given class
    ///     context.
    /// </returns>
    public static ILogger ForContext(Type source)
    {
        return Log.ForContext(source);
    }

    /// <summary>
    ///     Sanitizes a log message by replacing line endings with spaces to prevent
    ///     log injection attacks and maintain log integrity.
    /// </summary>
    /// <param name="message">The message to sanitize</param>
    /// <returns>The sanitized message</returns>
    public static string Sanitize(string message)
    {
        return message.ReplaceLineEndings(" ");
    }
}
