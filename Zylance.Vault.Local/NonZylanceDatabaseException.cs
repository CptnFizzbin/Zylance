namespace Zylance.Vault.Local;

/// <summary>
///     Exception thrown when attempting to open a database that is not a Zylance vault.
///     This protects against accidentally overwriting or corrupting non-Zylance databases.
/// </summary>
public class NonZylanceDatabaseException : Exception
{
    public NonZylanceDatabaseException(string filePath, string reason)
        : base($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}")
    {
        Reason = reason;
    }

    public NonZylanceDatabaseException(string filePath, string reason, Exception innerException)
        : base($"The database at '{filePath}' is not a Zylance vault. Reason: {reason}", innerException)
    {
        Reason = reason;
    }

    public static NonZylanceDatabaseException InvalidFile(string filePath) =>
        new(
            filePath,
            "Database file could not be opened. The file may be corrupt or is not a valid SQLite database."
        );

    public static NonZylanceDatabaseException InvalidFile(string filePath, Exception innerException) =>
        new(
            filePath,
            "Database file could not be opened. The file may be corrupt or is not a valid SQLite database.",
            innerException
        );


    public string Reason { get; }
}
