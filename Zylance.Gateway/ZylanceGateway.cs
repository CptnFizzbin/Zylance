using Zylance.Contract.Envelope;
using Zylance.Gateway.Handlers;
using Zylance.Gateway.Services;
using Zylance.Gateway.Transports;
using Zylance.Lib.Providers;
using Zylance.Lib.Serializers;

namespace Zylance.Gateway;

public class ZylanceGateway
{
    private readonly List<IRequestHandler> _requestHandlers = [];
    private readonly ITransport _transport;
    public readonly ActionHandler ActionHandler = new();
    public readonly EventMessageHandler EventHandler = new();

    public readonly FileService FileService;
    public readonly VaultService VaultService;

    public ZylanceGateway(
        ITransport transport,
        IFileProvider fileProvider,
        IVaultProvider vaultProvider
    )
    {
        _transport = transport;
        _transport.Receive(HandleMessage);

        FileService = new FileService(fileProvider);
        _requestHandlers.Add(new FileServiceHandler(FileService));

        VaultService = new VaultService(vaultProvider);
        _requestHandlers.Add(new VaultServiceHandler(VaultService));

        _requestHandlers.Add(ActionHandler);
    }

    public void Send(ResponsePayload response)
    {
        Console.WriteLine($"<== Res[{response.RequestId}]: {response.Status} - {response.DataJson}");
        var envelope = new GatewayEnvelope { Response = response };
        Send(envelope);
    }

    public void Send(EventPayload eventPayload)
    {
        Console.WriteLine($"<== Event: {eventPayload.Event} - {eventPayload.DataJson}");
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

    private void HandleMessage(string json)
    {
        var message = GatewayEnvelope.Parser.ParseJson(json);
        try
        {
            switch (message.PayloadCase)
            {
                case GatewayEnvelope.PayloadOneofCase.Request:
                    HandleRequest(message.Request);
                    break;
                case GatewayEnvelope.PayloadOneofCase.Event:
                    EventHandler.HandleEvent(message.Event);
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

            HandleException(ex, requestId);
        }
    }

    private void HandleRequest(RequestPayload request)
    {
        Console.WriteLine($"==> Req[{request.RequestId}]: {request.Action} - {request.DataJson}");
        var handler = _requestHandlers.FirstOrDefault(h => h.IsRequestHandled(request));
        if (handler is null)
            throw new NotSupportedException($"No handler for request action '{request.Action}'.");

        Send(handler.HandleRequest(request));
    }

    /// <summary>
    ///     Handles an exception by wrapping it in an ErrorPayload and sending it.
    /// </summary>
    private void HandleException(Exception ex, string? requestId = null)
    {
        var error = ExceptionHandler.WrapException(ex, requestId);
        Send(error);
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.NewGuid().ToString();
        var msgJson = MessageSerializer.Serialize(envelope);
        _transport.Send(msgJson);
    }
}
