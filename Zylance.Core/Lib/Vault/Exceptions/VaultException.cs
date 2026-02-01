namespace Zylance.Core.Lib.Vault.Exceptions;

public class VaultException(string message) : Exception(message)
{
    public static VaultException NoActiveVault()
    {
        return new VaultException("No active vault. Please open or create a vault before performing operations.");
    }

    public static VaultException VaultLocked()
    {
        return new VaultException("Vault is locked. Please unlock the vault before performing operations.");
    }
}
