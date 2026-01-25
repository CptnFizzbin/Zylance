using JetBrains.Annotations;

namespace Zylance.Core.Controllers.Attributes;

/// <summary>
///     Marks a method as a request handler for the specified action.
///     The RequestRouter will automatically discover and register methods with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
public class RequestHandlerAttribute : Attribute
{
    /// <summary>
    ///     Creates a request handler that will auto-detect the action name from the protobuf message types.
    ///     This requires the request and response types to have the [action] custom option defined.
    /// </summary>
    public RequestHandlerAttribute()
    {
        Action = null; // Will be resolved during registration
    }

    /// <summary>
    ///     Creates a request handler for the specified action.
    /// </summary>
    /// <param name="action">The action name this handler responds to.</param>
    public RequestHandlerAttribute(string action)
    {
        Action = action;
    }

    /// <summary>
    ///     The action name this handler responds to.
    ///     If null, the action will be auto-detected from the method's request/response types.
    /// </summary>
    public string? Action { get; }
}
