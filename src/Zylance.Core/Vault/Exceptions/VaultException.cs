namespace Zylance.Core.Vault.Exceptions;

/// <summary>
///     Exception type used for vault-related errors.
/// </summary>
public class VaultException(string message) : Exception(message)
{
    /// <summary>
    ///     Returns an exception indicating that no active vault is available.
    /// </summary>
    public static VaultException NoActiveVault()
    {
        return new VaultException("No active vault. Please open or create a vault before performing operations.");
    }

    /// <summary>
    ///     Returns an exception indicating the vault is currently locked.
    /// </summary>
    public static VaultException VaultLocked()
    {
        return new VaultException("Vault is locked. Please unlock the vault before performing operations.");
    }
}
