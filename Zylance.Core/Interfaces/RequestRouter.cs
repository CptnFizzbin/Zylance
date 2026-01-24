using Zylance.Core.Delegates;
using Zylance.Core.Models;
using Zylance.Core.Utils;

namespace Zylance.Core.Interfaces;

public class RequestRouter
{
    private readonly Dictionary<string, AsyncZyRequestHandler> _handlers = [];

    /// <summary>
    /// Registers an async request handler for the specified action.
    /// </summary>
    public RequestRouter Use(string action, AsyncZyRequestHandler handler)
    {
        _handlers.Add(action, handler);
        return this;
    }

    /// <summary>
    /// Registers a strongly-typed async request handler for the specified action.
    /// The handler will be wrapped to work with the generic handler interface.
    /// </summary>
    public RequestRouter Use<TReq, TRes>(string action, AsyncZyRequestHandler<TReq, TRes> handler)
    {
        return Use(action, RequestHandlerUtils.Wrap(handler));
    }

    public async Task<ZyResponse> MessageReceived(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        if (_handlers.TryGetValue(zyRequest.Action, out var handler))
            return await handler(zyRequest, zyResponse);

        return zyResponse;
    }
}
