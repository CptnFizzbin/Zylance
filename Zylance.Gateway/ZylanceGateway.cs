using Zylance.Contract;

namespace Zylance.Gateway;

public class ZylanceGateway
{
    private readonly Dictionary<string, Func<RequestPayload, ResponsePayload>> _requestHandlers = new();
    private readonly ITransport _transport;

    public ZylanceGateway(ITransport transport)
    {
        _transport = transport;
        _transport.Receive(HandleMessage);
    }

    public void OnRequest(string action, Func<RequestPayload, ResponsePayload> handler)
    {
        _requestHandlers[action] = handler;
    }

    public void Send(ResponsePayload response)
    {
        var envelope = new GatewayEnvelope { Response = response };
        Send(envelope);
    }

    public void Send(EventPayload eventPayload)
    {
        var envelope = new GatewayEnvelope { Event = eventPayload };
        Send(envelope);
    }

    public void Send(ErrorPayload errorPayload)
    {
        var envelope = new GatewayEnvelope { Error = errorPayload };
        Send(envelope);
    }

    private void HandleMessage(string json)
    {
        Console.WriteLine($"==> {json}");
        var message = GatewayEnvelope.Parser.ParseJson(json);
        switch (message.PayloadCase)
        {
            case GatewayEnvelope.PayloadOneofCase.Request:
                HandleRequest(message.Request);
                break;
            case GatewayEnvelope.PayloadOneofCase.Event:
            case GatewayEnvelope.PayloadOneofCase.None:
            case GatewayEnvelope.PayloadOneofCase.Response:
            case GatewayEnvelope.PayloadOneofCase.Error:
            default:
                throw new Exception("Unsupported message type received.");
        }
    }

    private void HandleRequest(RequestPayload request)
    {
        var action = request.Action;
        if (_requestHandlers.TryGetValue(action, out var handler))
            Send(handler.Invoke(request));
        else
            Send(
                new ErrorPayload
                {
                    RequestId = request.RequestId,
                    Type = "NotFound",
                    Details = $"No handler for action '{action}' found.",
                }
            );
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.NewGuid().ToString();
        var msgJson = MessageSerializer.Serialize(envelope);
        Console.WriteLine($"<== {msgJson}");
        _transport.Send(msgJson);
    }
}
