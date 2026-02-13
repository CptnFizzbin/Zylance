namespace Zylance.Core.Importers.Ofx.Models;

/// <summary>
/// OFX account type constants used by the OFX importer (e.g. CHECKING, SAVINGS).
/// </summary>
public static class OfxAccountType
{
    /// <summary>Checking account type.</summary>
    public const string Checking = "CHECKING";

    /// <summary>Savings account type.</summary>
    public const string Savings = "SAVINGS";

    /// <summary>Credit card account type.</summary>
    public const string CreditCard = "CREDITCARD";

    /// <summary>Investment account type.</summary>
    public const string Investment = "INVESTMENT";
}
