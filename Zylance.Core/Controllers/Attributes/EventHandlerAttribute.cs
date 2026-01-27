using JetBrains.Annotations;

namespace Zylance.Core.Controllers.Attributes;

/// <summary>
///     Marks a method as a request handler for the specified action.
///     The RequestRouter will automatically discover and register methods with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class EventHandlerAttribute : Attribute
{
    /// <summary>
    ///     The event name this handler responds to.
    ///     If null, the action will be auto-detected from the method's request/response types.
    /// </summary>
    [UsedImplicitly(Reason = "Used by source generator")]
    public string? EventName { get; init; }
}
