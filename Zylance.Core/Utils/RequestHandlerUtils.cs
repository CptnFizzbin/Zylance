using Zylance.Core.Delegates;
using Zylance.Core.Models;

namespace Zylance.Core.Utils;

public static class RequestHandlerUtils
{
    /// <summary>
    /// Returns a no-op handler that simply returns the response unchanged.
    /// Useful for skipping optional middleware handlers.
    /// </summary>
    public static AsyncZyRequestHandler Skip()
    {
        return (_, res) => Task.FromResult(res);
    }

    /// <summary>
    /// Wraps a strongly-typed handler into a generic AsyncZyRequestHandler.
    /// Handles the type conversions automatically.
    /// </summary>
    public static AsyncZyRequestHandler Wrap<TReq, TRes>(AsyncZyRequestHandler<TReq, TRes> handler)
    {
        return async (req, res) =>
        {
            var response = await handler(
                new ZyRequest<TReq> { Payload = req.Payload },
                new ZyResponse<TRes> { Payload = res.Payload }
            );

            return new ZyResponse { Payload = response.Payload };
        };
    }
}
