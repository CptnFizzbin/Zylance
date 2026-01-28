using JetBrains.Annotations;

namespace Zylance.Core.Lib.Gateway.Attributes;

/// <summary>
///     Marks a class as a request controller that should be automatically discovered and registered
///     by the RequestRouter. All methods marked with [RequestHandler] in this class will be registered.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class ControllerAttribute : Attribute
{
}
