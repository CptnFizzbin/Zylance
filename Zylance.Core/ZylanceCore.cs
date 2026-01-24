using Zylance.Contract.Envelope;
using Zylance.Contract.Events.Vault;
using Zylance.Contract.Messages.Echo;
using Zylance.Core.Controllers;
using Zylance.Gateway;
using Zylance.Lib.Models;
using Zylance.Lib.Serializers;

namespace Zylance.Core;

public class ZylanceCore
{
    private ZylanceGateway? _gateway;
    private IVault? _vault;

    public void Listen(ZylanceGateway gateway)
    {
        _gateway = gateway;

        gateway.ActionHandler.OnRequest(
            "Status/GetStatus",
            req => Respond(req.RequestId, StatusController.GetStatus())
        );

        gateway.ActionHandler.OnRequest(
            "Echo/Echo",
            req => Respond(req.RequestId, EchoController.Echo(GetRequestData<EchoReq>(req)))
        );

        gateway.EventHandler.OnEvent(
            "Vault/Opened",
            payload =>
            {
                var vaultRef = GetEventData<VaultOpenedEvt>(payload).Vault;
                _vault = _gateway.VaultService.GetVault(vaultRef);
            }
        );
    }

    private static TData GetEventData<TData>(EventPayload request)
    {
        return MessageSerializer.Deserialize<TData>(request.DataJson)
            ?? throw new ArgumentException($"Event payload is invalid: {request.DataJson}");
    }

    private static TData GetRequestData<TData>(RequestPayload request)
    {
        return MessageSerializer.Deserialize<TData>(request.DataJson)
            ?? throw new ArgumentException($"Request payload is invalid: {request.DataJson}");
    }

    private static ResponsePayload Respond(string requestId, Response response)
    {
        return new ResponsePayload
        {
            RequestId = requestId,
            Status = response.Status,
        };
    }

    private static ResponsePayload Respond<TData>(string requestId, ResponseWithData<TData> response)
    {
        return new ResponsePayload
        {
            RequestId = requestId,
            Status = response.Status,
            DataJson = MessageSerializer.Serialize(response.Data),
        };
    }
}
