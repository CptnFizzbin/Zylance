using Serilog;
using Zylance.Core.Logging;

namespace Zylance.Vault.Local;

/// <summary>
///     Exception thrown when attempting to open a database that is not a Zylance
///     vault.
///     This protects against accidentally overwriting or corrupting non-Zylance
///     databases.
/// </summary>
public class NonZylanceDatabaseException : Exception
{
    private static readonly ILogger Log = ZyLogger.ForContext<NonZylanceDatabaseException>();

    /// <summary>
    ///     Creates an exception for non-Zylance database errors.
    /// </summary>
    /// <param name="filePath">Path to the database file.</param>
    /// <param name="reason">Explanation for the error.</param>
    public NonZylanceDatabaseException(string filePath, string reason)
        : base($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}")
    {
        Reason = reason;
    }

    /// <summary>
    ///     Creates an exception for non-Zylance database errors with an inner
    ///     exception.
    /// </summary>
    /// <param name="filePath">Path to the database file.</param>
    /// <param name="reason">Explanation for the error.</param>
    /// <param name="innerException">The underlying exception that caused this error.</param>
    public NonZylanceDatabaseException(string filePath, string reason, Exception innerException)
        : base($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}", innerException)
    {
        Reason = reason;
    }

    /// <summary>
    ///     Human-readable reason why the database is considered non-Zylance.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    ///     Helper to create an exception indicating the file is invalid or not a
    ///     SQLite DB.
    /// </summary>
    public static NonZylanceDatabaseException InvalidFile(string filePath)
    {
        return new NonZylanceDatabaseException(
            filePath,
            "Database file could not be opened. The file may be corrupt or is not a valid SQLite database."
        );
    }

    /// <summary>
    ///     Helper to create an exception indicating the file is invalid, preserving
    ///     the inner exception.
    /// </summary>
    public static NonZylanceDatabaseException InvalidFile(string filePath, Exception innerException)
    {
        return new NonZylanceDatabaseException(
            filePath,
            "Database file could not be opened. The file may be corrupt or is not a valid SQLite database.",
            innerException
        );
    }
}
