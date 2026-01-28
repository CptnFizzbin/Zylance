using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Handlers;
using Zylance.Core.Lib.Gateway.Models;
using Zylance.Core.Lib.Gateway.Services;
using Zylance.Core.Lib.Gateway.Utils;
using Zylance.Core.Lib.Interfaces;

namespace Zylance.Core.Lib.Gateway;

public class Gateway
{
    private readonly RouterService _routerService;
    private readonly ITransport _transport;

    public Gateway(ITransport transport, RouterService routerService)
    {
        _transport = transport;
        _routerService = routerService;
        _transport.Receive(message => _ = HandleMessage(message));
    }

    public void Send(ResponsePayload response)
    {
        Console.WriteLine($"<== Res[{response.RequestId}]: {response.Status} - {response.DataJson}");
        var envelope = new GatewayEnvelope { Response = response };
        Send(envelope);
    }

    public void Send(EventPayload eventPayload)
    {
        Console.WriteLine($"<== Evt: {eventPayload.Event} - {eventPayload.DataJson}");
        var envelope = new GatewayEnvelope { Event = eventPayload };
        Send(envelope);
    }

    public void Send(ErrorPayload errorPayload)
    {
        Console.WriteLine(
            errorPayload.HasRequestId
                ? $"<== ERR[{errorPayload.RequestId}]: {errorPayload.Type} - {errorPayload.Details}"
                : $"<== ERR: {errorPayload.Type} - {errorPayload.Details}"
        );

        var envelope = new GatewayEnvelope { Error = errorPayload };
        Send(envelope);
    }

    private async Task HandleMessage(string json)
    {
        var message = GatewayEnvelope.Parser.ParseJson(json);
        try
        {
            switch (message.PayloadCase)
            {
                case GatewayEnvelope.PayloadOneofCase.Request:
                    await HandleMessage(message.Request);
                    break;
                case GatewayEnvelope.PayloadOneofCase.Event:
                    await HandleMessage(message.Event);
                    break;
                case GatewayEnvelope.PayloadOneofCase.Response:
                case GatewayEnvelope.PayloadOneofCase.Error:
                case GatewayEnvelope.PayloadOneofCase.None:
                default:
                    throw new NotSupportedException("Unsupported message type received.");
            }
        }
        catch (Exception ex)
        {
            var requestId = message.PayloadCase == GatewayEnvelope.PayloadOneofCase.Request
                ? message.Request.RequestId
                : null;

            var error = ExceptionHandler.WrapException(ex, requestId);
            Send(error);
        }
    }

    private async Task HandleMessage(RequestPayload reqPayload)
    {
        Console.WriteLine($"==> Req[{reqPayload.RequestId}]: {reqPayload.Action} - {reqPayload.DataJson}");

        var req = new ZyRequest { Payload = reqPayload };

        var resPayload = new ResponsePayload { RequestId = reqPayload.RequestId };
        var res = new ZyResponse { Payload = resPayload };

        res = await _routerService.HandleRequest(req, res);
        Send(res.Payload);
    }

    private async Task HandleMessage(EventPayload payload)
    {
        Console.WriteLine($"==> Evt: {payload.Event} - {payload.DataJson}");

        var evt = new ZyEvent { Payload = payload };

        await _routerService.HandleEvent(evt);
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.NewGuid().ToString();
        var msgJson = MessageSerializer.Serialize(envelope);
        _transport.Send(msgJson);
    }
}
