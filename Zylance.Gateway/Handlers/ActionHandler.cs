using Zylance.Contract.Envelope;

namespace Zylance.Gateway.Handlers;

public class ActionHandler : IRequestHandler
{
    private readonly Dictionary<string, Func<RequestPayload, ResponsePayload>> _actionHandlers = new();

    public bool IsRequestHandled(RequestPayload request)
    {
        return HasHandler(request.Action);
    }

    public ResponsePayload HandleRequest(RequestPayload request)
    {
        return GetHandler(request.Action).Invoke(request);
    }

    public void OnRequest(string action, Func<RequestPayload, ResponsePayload> handler)
    {
        _actionHandlers[action] = handler;
    }

    private bool HasHandler(string action)
    {
        return _actionHandlers.ContainsKey(action);
    }

    private Func<RequestPayload, ResponsePayload> GetHandler(string action)
    {
        return !HasHandler(action)
            ? throw new InvalidOperationException($"No handler registered for action '{action}'.")
            : _actionHandlers[action];
    }
}
