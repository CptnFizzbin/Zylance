using Google.Protobuf;
using JetBrains.Annotations;
using Serilog;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;

namespace Zylance.Core.Gateway.Utils;

/// <summary>
///     Utility helpers for wrapping strongly-typed request handlers into generic
///     handlers used by generated controllers.
/// </summary>
public static class RequestHandlerUtils
{
    private static readonly ILogger Log = ZyLogger.ForContext(typeof(RequestHandlerUtils));

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
            Log.Debug(
                "Invoking wrapped async request handler for RequestType={ReqType} ResponseType={ResType}",
                typeof(TReq).FullName,
                typeof(TRes).FullName
            );
            var typedReq = new ZyRequest<TReq> { Payload = req.Payload };
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload, OnSend = res.OnSend };

            await handler(typedReq, typedRes);

            Log.Debug("Async request handler completed for RequestType={ReqType}", typeof(TReq).FullName);
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
            Log.Debug(
                "Invoking wrapped sync request handler for RequestType={ReqType} ResponseType={ResType}",
                typeof(TReq).FullName,
                typeof(TRes).FullName
            );
            var typedReq = new ZyRequest<TReq> { Payload = req.Payload };
            var typedRes = new ZyResponse<TRes> { Payload = res.Payload, OnSend = res.OnSend };

            handler(typedReq, typedRes);

            Log.Debug("Sync request handler completed for RequestType={ReqType}", typeof(TReq).FullName);
            return Task.FromResult<ZyResponse>(typedRes);
        };
    }
}
