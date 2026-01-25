using Zylance.Core.Delegates;
using Zylance.Core.Models;

namespace Zylance.Core.Utils;

public static class RequestHandlerUtils
{
    /// <summary>
    ///     Wraps a strongly-typed async handler (returns Task) into a generic AsyncZyRequestHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    public static AsyncZyRequestHandler Wrap<TReq, TRes>(AsyncZyRequestHandler<TReq, TRes> handler)
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
    public static AsyncZyRequestHandler WrapSync<TReq, TRes>(SyncZyRequestHandler<TReq, TRes> handler)
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
