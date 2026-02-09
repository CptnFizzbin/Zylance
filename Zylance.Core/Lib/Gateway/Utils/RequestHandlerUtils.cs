using Google.Protobuf;
using JetBrains.Annotations;
using Zylance.Core.Lib.Gateway.Handlers;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.Lib.Gateway.Utils;

public static class RequestHandlerUtils
{
    /// <summary>
    ///     Wraps a strongly-typed async handler (returns Task) into a generic AsyncZyRequestHandler.
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
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload };

            await handler(typedReq, typedRes);

            return new ZyResponse { Payload = typedRes.Payload };
        };
    }

    /// <summary>
    ///     Wraps a strongly-typed sync handler (returns void) into a generic AsyncZyRequestHandler.
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
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload };

            handler(typedReq, typedRes);

            return Task.FromResult(new ZyResponse { Payload = typedRes.Payload });
        };
    }
}
