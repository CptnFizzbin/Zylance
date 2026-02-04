using Zylance.Core.Lib.Importers.Ofx.V1.Raw;

namespace Zylance.Core.Lib.Importers.Ofx.Elements;

public record OfxRoot
{
    public required OfxSignOnMessage SignOnMessage { get; init; }
    public OfxBankMessage? BankMessage { get; init; }
    public OfxCreditCardMessage? CreditCardMessage { get; init; }

    internal static OfxRoot FromRaw(OfxRawElement rawFileRoot)
    {
        return new OfxRoot
        {
            SignOnMessage = OfxSignOnMessage.FromRaw(rawFileRoot.GetChildElement("SIGNONMSGSRSV1")),
            BankMessage = OfxBankMessage.FromRaw(rawFileRoot.GetChildElement("BANKMSGSRSV1")),
            CreditCardMessage = OfxCreditCardMessage.FromRaw(rawFileRoot.GetChildElement("CREDITCARDMSGSRSV1")),
        };
    }
}
