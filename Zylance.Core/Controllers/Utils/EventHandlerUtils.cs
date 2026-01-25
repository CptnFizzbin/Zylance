using Zylance.Core.Delegates;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers.Utils;

public static class EventHandlerUtils
{
    /// <summary>
    ///     Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    public static AsyncZyEventHandler Wrap<TData>(AsyncZyEventHandler<TData> handler)
    {
        return evt => handler(new ZyEvent<TData> { Payload = evt.Payload });
    }

    /// <summary>
    ///     Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    ///     Handles the type conversions automatically.
    /// </summary>
    public static AsyncZyEventHandler WrapSync<TData>(SyncZyEventHandler<TData> handler)
    {
        return evt =>
        {
            var typedEvt = new ZyEvent<TData> { Payload = evt.Payload };

            handler(typedEvt);

            return Task.CompletedTask;
        };
    }
}
