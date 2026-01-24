using Zylance.Core.Delegates;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;

namespace Zylance.Core.Utils;

public static class EventHandlerUtils
{
    /// <summary>
    /// Returns a no-op event handler.
    /// Useful for skipping optional event handlers.
    /// </summary>
    public static AsyncZyEventHandler Skip()
    {
        return _ => Task.CompletedTask;
    }

    /// <summary>
    /// Wraps a strongly-typed event handler into a generic AsyncZyEventHandler.
    /// Handles the type conversions automatically.
    /// </summary>
    public static AsyncZyEventHandler Wrap<TData>(AsyncZyEventHandler<TData> handler)
    {
        return evt => handler(new ZyEvent<TData> { Payload = evt.Payload });
    }
}
