using Zylance.Contract;

namespace Zylance.Gateway;

public class ZylanceGateway
{
    private readonly FileService _fileService;
    private readonly FileServiceHandler _fileServiceHandler;
    private readonly Dictionary<string, Func<RequestPayload, ResponsePayload>> _requestHandlers = new();
    private readonly ITransport _transport;

    public ZylanceGateway(ITransport transport, IFileProvider fileProvider)
    {
        _transport = transport;
        _fileService = new FileService(fileProvider);
        _fileServiceHandler = new FileServiceHandler(_fileService);
        _transport.Receive(HandleMessage);
    }

    public FileService GetFileService()
    {
        return _fileService;
    }

    public void OnRequest(string action, Func<RequestPayload, ResponsePayload> handler)
    {
        _requestHandlers[action] = handler;
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
        Console.WriteLine($"<== ERROR: {errorPayload.Type} - {errorPayload.Details}");
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
                case GatewayEnvelope.PayloadOneofCase.None:
                case GatewayEnvelope.PayloadOneofCase.Response:
                case GatewayEnvelope.PayloadOneofCase.Error:
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
        var action = request.Action;

        if (action.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            var response = _fileServiceHandler.HandleFileRequest(request);
            Send(response);
        }
        else if (_requestHandlers.TryGetValue(action, out var handler))
        {
            Send(handler.Invoke(request));
        }
        else
        {
            throw new NotSupportedException($"No handler for action '{action}' found.");
        }
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
