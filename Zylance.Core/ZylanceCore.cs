using Zylance.Contract;
using Zylance.Core.Controllers;
using Zylance.Gateway;

namespace Zylance.Core;

public class ZylanceCore
{
    private ZylanceCore(ZylanceGateway gateway)
    {
        gateway.OnRequest(
            "Status/GetStatus",
            req => Respond(req.RequestId, StatusController.GetStatus())
        );

        gateway.OnRequest(
            "Echo/Echo",
            req => Respond(req.RequestId, EchoController.Echo(GetRequestData<EchoReq>(req)))
        );
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

    public static void Listen(ZylanceGateway gateway)
    {
        _ = new ZylanceCore(gateway);
    }
}
