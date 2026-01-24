using Zylance.Contract.Envelope;
using Zylance.Contract.Messages.Vault;
using Zylance.Gateway.Services;
using Zylance.Lib.Serializers;

namespace Zylance.Gateway.Handlers;

public class VaultServiceHandler(VaultService vaultService) : IRequestHandler
{
    private const string Prefix = "vault:";

    public bool IsRequestHandled(RequestPayload request)
    {
        return request.Action.StartsWith(Prefix);
    }

    /// <summary>
    ///     Handles a file request by routing to the appropriate file operation.
    /// </summary>
    public ResponsePayload HandleRequest(RequestPayload request)
    {
        var action = request.Action[Prefix.Length..];

        return action switch
        {
            "openVault" => HandleOpenVault(request),
            "createVault" => HandleCreateVault(request),
            _ => throw new NotSupportedException($"vault action '{action}' is not supported."),
        };
    }

    private ResponsePayload HandleOpenVault(RequestPayload request)
    {
        var vaultRef = vaultService.OpenVault();
        return new ResponsePayload
        {
            RequestId = request.RequestId,
            Status = "success",
            DataJson = MessageSerializer.Serialize(new VaultOpenRes { Vault = vaultRef }),
        };
    }

    private ResponsePayload HandleCreateVault(RequestPayload request)
    {
        var vaultRef = vaultService.CreateVault();
        return new ResponsePayload
        {
            RequestId = request.RequestId,
            Status = "success",
            DataJson = MessageSerializer.Serialize(new VaultCreateRes { Vault = vaultRef }),
        };
    }
}
