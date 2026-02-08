using System.Collections.Concurrent;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Handlers;
using Zylance.Core.Lib.Gateway.Models;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Services;

public class GatewayService
{
    private readonly ConcurrentDictionary<string, List<ZyEventSubscription>> _eventListeners = new();
    private readonly RouterService _routerService;
    private readonly ITransport _transport;

    public GatewayService(ITransport transport, RouterService routerService)
    {
        _transport = transport;
        _routerService = routerService;
        _transport.Receive(message => _ = HandleMessage(message));

        SubscribeToEvent(
            "Vault:VaultClosed",
            _ =>
            {
                Console.WriteLine("Vault closed. Clearing event listeners.");
                _eventListeners.Clear();
            }
        );
    }

    public void Send(ResponsePayload response)
    {
        Console.WriteLine($"<== Res[{response.RequestId}]: {response.DataJson}");
        var envelope = new GatewayEnvelope { Response = response };
        Send(envelope);
    }

    public void Send(EventPayload eventPayload)
    {
        Console.WriteLine($"<== Evt: {eventPayload.EventName} - {eventPayload.DataJson}");
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

    public EventObservable ObserveEvent(string eventName)
    {
        return new EventObservable(this, eventName);
    }

    public Subscription SubscribeToEvent(string eventName, Action<ZyEvent> handler)
    {
        var listenerId = Guid.NewGuid();

        var listener = new ZyEventSubscription
        {
            Id = listenerId,
            EventName = eventName,
            Handler = handler,
            Unsubscribe = () => RemoveEventListener(eventName, listenerId),
        };

        AddEventListener(eventName, listener);

        return listener;
    }

    private void AddEventListener(string eventName, ZyEventSubscription listener)
    {
        _eventListeners.AddOrUpdate(
            eventName,
            _ => [listener],
            (_, list) =>
            {
                list.Add(listener);
                return list;
            }
        );
    }

    private void RemoveEventListener(string eventName, Guid listenerId)
    {
        _eventListeners.AddOrUpdate(
            eventName,
            _ => [],
            (_, list) =>
            {
                list.RemoveAll(l => l.Id == listenerId);
                return list;
            }
        );
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
            var requestId =
                message.PayloadCase == GatewayEnvelope.PayloadOneofCase.Request ? message.Request.RequestId : null;

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
        Console.WriteLine($"==> Evt: {payload.EventName} - {payload.DataJson}");

        var evt = new ZyEvent { Payload = payload };

        if (_eventListeners.TryGetValue(payload.EventName, out var listeners))
            // Iterate over a copy to avoid collection modified exception
            // when handlers unsubscribe during invocation (e.g., ObserveEvent().FirstAsync())
            foreach (var listener in listeners.ToList())
                listener.Handler(evt);

        await _routerService.HandleEvent(evt);
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.CreateVersion7().ToString();
        var msgJson = MessageUtils.ToJson(envelope);
        _transport.Send(msgJson);
    }
}
