using Google.Protobuf;
using JetBrains.Annotations;
using Zylance.Core.Gateway.Handlers;
using Zylance.Core.Gateway.Models;

namespace Zylance.Core.Gateway.Utils;

/// <summary>
///     Utilities for wrapping event handlers for generated controllers.
/// </summary>
public static class EventHandlerUtils
{
    /// <summary>
    ///     Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    [UsedImplicitly(Reason = "Used by controllers via source generator")]
    public static AsyncZyEventHandler Wrap<TData>(AsyncZyEventHandler<TData> handler)
        where TData : IMessage, new()
    {
        return evt => handler(new ZyEvent<TData> { Payload = evt.Payload });
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
            var typedEvt = new ZyEvent<TData> { Payload = evt.Payload };

            handler(typedEvt);

            return Task.CompletedTask;
        };
    }
}
