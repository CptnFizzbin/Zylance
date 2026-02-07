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

    public string Reason { get; }
}
