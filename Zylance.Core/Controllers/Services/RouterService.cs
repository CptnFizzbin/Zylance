using JetBrains.Annotations;
using Zylance.Core.Delegates;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers.Services;

public class RouterService
{
    private readonly Dictionary<string, AsyncZyEventHandler> _eventHandlers = [];
    private readonly Dictionary<string, AsyncZyRequestHandler> _requestHandlers = [];

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    [UsedImplicitly(Reason = "Called by generated code.")]
    public RouterService Use(string action, AsyncZyRequestHandler handler)
    {
        _requestHandlers.Add(action, handler);
        return this;
    }

    /// <summary>
    ///     Registers an async request handler for the specified action.
    /// </summary>
    [UsedImplicitly(Reason = "Called by generated code.")]
    public RouterService Use(string eventName, AsyncZyEventHandler handler)
    {
        _eventHandlers.Add(eventName, handler);
        return this;
    }

    public async Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_requestHandlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }

    public async Task HandleEvent(ZyEvent zyEvent)
    {
        if (_eventHandlers.TryGetValue(zyEvent.Name, out var handler))
            await handler(zyEvent);
    }
}
