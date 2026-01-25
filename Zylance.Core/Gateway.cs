using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Delegates;
using Zylance.Core.Handlers;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;
using Zylance.Core.Serializers;

namespace Zylance.Core;

public class Gateway
{
    private readonly HashSet<AsyncZyEventHandler> _eventHandlers = [];
    private readonly RequestRouter _router;
    private readonly ITransport _transport;

    public Gateway(ITransport transport, RequestRouter router)
    {
        _transport = transport;
        _router = router;
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

        var resPayload = new ResponsePayload { RequestId = reqPayload.RequestId };
        var request = new ZyRequest { Payload = reqPayload };
        var response = new ZyResponse { Payload = resPayload };

        // Route the request through the centralized router
        response = await _router.MessageReceived(request, response);

        Send(response.Payload);
    }

    private async Task HandleMessage(EventPayload payload)
    {
        Console.WriteLine($"==> Evt: {payload.Event} - {payload.DataJson}");
        var zyEvent = new ZyEvent { Payload = payload };

        foreach (var handler in _eventHandlers)
            await handler(zyEvent);
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.NewGuid().ToString();
        var msgJson = MessageSerializer.Serialize(envelope);
        _transport.Send(msgJson);
    }
}
