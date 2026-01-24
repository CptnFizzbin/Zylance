using Zylance.Contract.Envelope;

namespace Zylance.Gateway.Handlers;

public class EventMessageHandler
{
    private readonly Dictionary<string, List<Action<EventPayload>>> _eventHandlers = new();

    public void HandleEvent(EventPayload eventPayload)
    {
        if (!_eventHandlers.TryGetValue(eventPayload.Event, out var handlers))
            return;

        foreach (var handler in handlers)
            handler(eventPayload);
    }

    public void OnEvent(string eventName, Action<EventPayload> handler)
    {
        var handlers = _eventHandlers.TryGetValue(eventName, out var value)
            ? value
            : [];

        handlers.Add(handler);

        _eventHandlers[eventName] = handlers;
    }
}
