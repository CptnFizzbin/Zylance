namespace Zylance.Vault.Local;

/// <summary>
///     Exception thrown when attempting to open a database that is not a Zylance vault.
///     This protects against accidentally overwriting or corrupting non-Zylance databases.
/// </summary>
public class NonZylanceDatabaseException : Exception
{
    public string Reason { get; }

    public NonZylanceDatabaseException(string filePath)
        : base($"The database at '{filePath}' is not a Zylance vault.")
    {
        Reason = "The required '_zylance_' marker table was not found.";
    }
}
