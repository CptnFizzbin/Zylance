namespace Zylance.Vault.Local;

/// <summary>
///     Exception thrown when attempting to open a database that is not a Zylance vault.
///     This protects against accidentally overwriting or corrupting non-Zylance databases.
/// </summary>
public class NonZylanceDatabaseException(string filePath)
    : Exception($"The database at '{filePath}' is not a Zylance vault.")
{
    public string Reason { get; } = "The required '_zylance_' marker table was not found.";
}
