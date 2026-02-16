using Google.Protobuf;
using JetBrains.Annotations;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;

namespace Zylance.Core.Gateway.Utils;

/// <summary>
///     Utility helpers for wrapping strongly-typed request handlers into generic
///     handlers used by generated controllers.
/// </summary>
public static class RequestHandlerUtils
{
    /// <summary>
    ///     Wraps a strongly-typed async handler (returns Task) into a generic
    ///     AsyncZyRequestHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    [UsedImplicitly(Reason = "Used by controllers via source generator")]
    public static AsyncZyRequestHandler Wrap<TReq, TRes>(AsyncZyRequestHandler<TReq, TRes> handler)
        where TRes : IMessage, new()
        where TReq : IMessage, new()
    {
        return async (req, res) =>
        {
            var typedReq = new ZyRequest<TReq> { Payload = req.Payload };
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload, OnSend = res.OnSend };

            await handler(typedReq, typedRes);

            return typedRes;
        };
    }

    /// <summary>
    ///     Wraps a strongly-typed sync handler (returns void) into a generic
    ///     AsyncZyRequestHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    [UsedImplicitly(Reason = "Used by controllers via source generator")]
    public static AsyncZyRequestHandler WrapSync<TReq, TRes>(SyncZyRequestHandler<TReq, TRes> handler)
        where TReq : IMessage, new()
        where TRes : IMessage, new()
    {
        return (req, res) =>
        {
            var typedReq = new ZyRequest<TReq> { Payload = req.Payload };
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload, OnSend = res.OnSend };

            handler(typedReq, typedRes);

            return Task.FromResult<ZyResponse>(typedRes);
        };
    }
}
