using Google.Protobuf;
using JetBrains.Annotations;
using Serilog;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;

namespace Zylance.Core.Gateway.Utils;

/// <summary>
///     Utilities for wrapping event handlers for generated controllers.
/// </summary>
public static class EventHandlerUtils
{
    private static readonly ILogger Log = ZyLogger.CreateLogger(typeof(EventHandlerUtils));

    /// <summary>
    ///     Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    [UsedImplicitly(Reason = "Used by controllers via source generator")]
    public static AsyncZyEventHandler Wrap<TData>(AsyncZyEventHandler<TData> handler)
        where TData : IMessage, new()
    {
        return evt =>
        {
            Log.Debug("Invoking wrapped async event handler for {Type}", typeof(TData).FullName);
            return handler(new ZyEvent<TData> { Payload = evt.Payload });
        };
    }

    /// <summary>
    ///     Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    [UsedImplicitly(Reason = "Used by controllers via source generator")]
    public static AsyncZyEventHandler WrapSync<TData>(SyncZyEventHandler<TData> handler)
        where TData : IMessage, new()
    {
        return evt =>
        {
            Log.Debug("Invoking wrapped sync event handler for {Type}", typeof(TData).FullName);
            var typedEvt = new ZyEvent<TData> { Payload = evt.Payload };

            handler(typedEvt);

            return Task.CompletedTask;
        };
    }
}
