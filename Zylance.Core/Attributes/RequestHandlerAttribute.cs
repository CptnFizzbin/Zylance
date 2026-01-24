using JetBrains.Annotations;

namespace Zylance.Core.Attributes;

/// <summary>
///     Marks a method as a request handler for the specified action.
///     The RequestRouter will automatically discover and register methods with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse(ImplicitUseKindFlags.Access)]
public class RequestHandlerAttribute(string action) : Attribute
{
    /// <summary>
    ///     The action name this handler responds to.
    /// </summary>
    public string Action { get; } = action;
}
