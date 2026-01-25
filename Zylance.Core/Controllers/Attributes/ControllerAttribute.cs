using JetBrains.Annotations;

namespace Zylance.Core.Attributes;

/// <summary>
///     Marks a class as a request controller that should be automatically discovered and registered
///     by the RequestRouter. All methods marked with [RequestHandler] in this class will be registered.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse(
    ImplicitUseKindFlags
        .InstantiatedNoFixedConstructorSignature)] // Tell ReSharper/Rider this class is instantiated via DI
public class ControllerAttribute : Attribute
{
}
