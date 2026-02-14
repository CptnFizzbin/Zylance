using JetBrains.Annotations;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;

namespace Zylance.Core.Router.Services;

/// <summary>
///     Lightweight routing service used by the gateway to map actions and event
///     names to handler delegates.
///     Generated registration code calls into this service to register
///     controllers.
/// </summary>
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

    /// <summary>
    ///     Handles an incoming ZyRequest by routing it to a registered handler.
    /// </summary>
    /// <param name="zyRequest">The incoming request.</param>
    /// <param name="zyResponse">The response object to populate.</param>
    public async Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_requestHandlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }

    /// <summary>
    ///     Handles an incoming ZyEvent by invoking registered event handlers.
    /// </summary>
    /// <param name="zyEvent">The incoming event.</param>
    public async Task HandleEvent(ZyEvent zyEvent)
    {
        if (_eventHandlers.TryGetValue(zyEvent.Name, out var handler))
            await handler(zyEvent);
    }
}
