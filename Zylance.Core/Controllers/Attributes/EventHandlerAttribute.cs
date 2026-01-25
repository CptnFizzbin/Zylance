using JetBrains.Annotations;

namespace Zylance.Core.Controllers.Attributes;

/// <summary>
///     Marks a method as a request handler for the specified action.
///     The RequestRouter will automatically discover and register methods with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
public class EventHandlerAttribute : Attribute
{
    /// <summary>
    ///     Creates an event handler that will auto-detect the event name from the protobuf message type.
    ///     This requires the event type to have the [eventName] custom option defined.
    /// </summary>
    public EventHandlerAttribute()
    {
        EventName = null; // Will be resolved during registration
    }

    /// <summary>
    ///     Creates an event handler for the specified event name.
    /// </summary>
    /// <param name="eventName">The event name this handler responds to.</param>
    public EventHandlerAttribute(string eventName)
    {
        EventName = eventName;
    }

    /// <summary>
    ///     The event name this handler responds to.
    ///     If null, the action will be auto-detected from the method's request/response types.
    /// </summary>
    public string? EventName { get; }
}
