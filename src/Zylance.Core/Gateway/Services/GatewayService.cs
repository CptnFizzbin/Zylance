using System.Collections.Concurrent;
using System.Reactive.Linq;
using Google.Protobuf;
using Serilog;
using Zylance.Contract;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Utils;
using Zylance.Core.Platform.Interfaces;

namespace Zylance.Core.Gateway.Services;

/// <summary>
///     Service that sends and receives messages
/// </summary>
public class GatewayService : IDisposable
{
    private readonly ConcurrentDictionary<string, HashSet<ZyEventSubscription>> _eventListeners = new();
    private readonly List<IObserver<EventPayload>> _eventObservers = [];
    private readonly List<IObserver<RequestPayload>> _requestObservers = [];
    private readonly ITransport _transport;

    /// <summary>
    ///     Initializes a new instance of <see cref="GatewayService" /> with the
    ///     specified transport and router.
    /// </summary>
    public GatewayService(ITransport transport)
    {
        _transport = transport;
        _transport.Receive(HandleMessage);

        ObserveEvents()
            .Where(evt => evt.EventName == ZylanceEvents.Vault_VaultClosed)
            .Take(1)
            .Subscribe(_ => Dispose());
    }

    /// <summary>
    ///     Cleans up resources used by the gateway, such as open connections and
    ///     subscriptions.
    /// </summary>
    public void Dispose()
    {
        foreach (var eventObserver in _eventObservers.ToArray())
            eventObserver.OnCompleted();

        foreach (var requestObserver in _requestObservers.ToArray())
            requestObserver.OnCompleted();

        _eventListeners.Clear();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Sends a response payload over the configured transport.
    /// </summary>
    public void Send(ResponsePayload response)
    {
        Log.Information((string)"<== Res[{RequestId}]: {DataJson}", response.RequestId, response.DataJson);
        var envelope = new GatewayEnvelope { Response = response };
        Send(envelope);
    }

    /// <summary>
    ///     Sends an event over the configured transport.
    /// </summary>
    public void SendEvent<TEvt>(TEvt evt)
        where TEvt : IMessage, new()
    {
        Send(MessageUtils.ToEventPayload(evt));
    }

    /// <summary>
    ///     Sends an event payload over the configured transport.
    /// </summary>
    public void Send(EventPayload eventPayload)
    {
        Log.Information((string)"<== Evt: {EventName} - {DataJson}", eventPayload.EventName, eventPayload.DataJson);
        var envelope = new GatewayEnvelope { Event = eventPayload };
        Send(envelope);
    }

    /// <summary>
    ///     Sends an error payload over the configured transport.
    /// </summary>
    public void Send(ErrorPayload errorPayload)
    {
        if (errorPayload.HasRequestId)
            Log.Information(
                "<== ERR[{RequestId}]: {Type} - {Details}",
                errorPayload.RequestId,
                errorPayload.Type,
                errorPayload.Details
            );
        else
            Log.Information((string)"<== ERR: {Type} - {Details}", errorPayload.Type, errorPayload.Details);

        var envelope = new GatewayEnvelope { Error = errorPayload };
        Send(envelope);
    }

    /// <summary>
    ///     Creates an observable that listens for all incoming requests. Consumers can
    ///     filter by action name or payload content as needed.
    /// </summary>
    /// <returns>Observable of RequestPayloads</returns>
    public IObservable<RequestPayload> ObserveRequests()
    {
        return Observable.Create<RequestPayload>(observer =>
        {
            _requestObservers.Add(observer);
            return () => _requestObservers.Remove(observer);
        });
    }

    /// <summary>
    ///     Creates an observable that listens for all incoming events. Consumers can
    ///     filter by event name or payload content as needed.
    /// </summary>
    /// <returns>Observable of EventPayloads</returns>
    public IObservable<EventPayload> ObserveEvents()
    {
        return Observable.Create<EventPayload>(observer =>
        {
            _eventObservers.Add(observer);
            return () => _eventObservers.Remove(observer);
        });
    }

    /// <summary>
    ///     Creates an observable that listens for events with the specified name.
    /// </summary>
    public EventObservable ObserveEvent(string eventName)
    {
        return new EventObservable(this, eventName);
    }

    /// <summary>
    ///     Creates an observable that listens for events with the specified name.
    /// </summary>
    public EventObservable<TData> ObserveEvent<TData>(string eventName)
        where TData : IMessage, new()
    {
        return ObserveEvent(eventName)
            .Select(evt => new ZyEvent<TData> { Payload = evt.Payload })
            .Select(evt => evt.Data);
    }

    /// <summary>
    ///     Subscribes the given handler to events with the specified name and returns
    ///     a subscription that can be disposed.
    /// </summary>
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
            _ => new HashSet<ZyEventSubscription> { listener },
            (_, set) =>
            {
                set.Add(listener);
                return set;
            }
        );
    }

    private void RemoveEventListener(string eventName, Guid listenerId)
    {
        _eventListeners.AddOrUpdate(
            eventName,
            _ => [],
            (_, set) =>
            {
                set.RemoveWhere(l => l.Id == listenerId);
                return set;
            }
        );
    }

    private void HandleMessage(string json)
    {
        var message = GatewayEnvelope.Parser.ParseJson(json);
        try
        {
            switch (message.PayloadCase)
            {
                case GatewayEnvelope.PayloadOneofCase.Request:
                    HandleMessage(message.Request);
                    break;
                case GatewayEnvelope.PayloadOneofCase.Event:
                    HandleMessage(message.Event);
                    break;
                case GatewayEnvelope.PayloadOneofCase.Response:
                case GatewayEnvelope.PayloadOneofCase.Error:
                case GatewayEnvelope.PayloadOneofCase.None:
                case GatewayEnvelope.PayloadOneofCase.Stream:
                default:
                    throw new NotSupportedException("Unsupported message type received.");
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, message.Request?.RequestId);
        }
    }

    /// <summary>
    ///     Handles exceptions that occur during message processing by wrapping them in
    ///     an
    ///     ErrorPayload and sending them over the transport. If a requestId is
    ///     provided,
    ///     it will be included in the error payload to correlate with the original
    ///     request.
    /// </summary>
    /// <param name="ex">The exception to handle</param>
    /// <param name="requestId">The related request id</param>
    public void HandleError(Exception ex, string? requestId = null)
    {
        var error = ExceptionHandler.WrapException(ex, requestId);
        Send(error);
    }

    private void HandleMessage(RequestPayload reqPayload)
    {
        Log.Information("==> Req: {Action} - {DataJson}", reqPayload.Action, reqPayload.DataJson);

        foreach (var observer in _requestObservers)
            observer.OnNext(reqPayload);
    }

    private void HandleMessage(EventPayload evtPayload)
    {
        Log.Information("==> Evt: {EventName} - {DataJson}", evtPayload.EventName, evtPayload.DataJson);

        foreach (var observer in _eventObservers)
            observer.OnNext(evtPayload);

        var evt = new ZyEvent { Payload = evtPayload };
        if (!_eventListeners.TryGetValue(evtPayload.EventName, out var listeners))
            return;

        // Iterate over a copy to avoid collection modified exception
        foreach (var listener in listeners.ToList())
            listener.Handler(evt);
    }

    private void Send(GatewayEnvelope envelope)
    {
        envelope.MessageId = Guid.CreateVersion7().ToString();
        var msgJson = MessageUtils.ToJson(envelope);
        _transport.Send(msgJson);
    }
}
