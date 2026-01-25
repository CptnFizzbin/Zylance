using System.Linq;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Analyzers;

/// <summary>
///     Diagnostic rules for controller registration source generator
/// </summary>
internal static class DiagnosticRules
{
    public static readonly DiagnosticDescriptor MissingControllerAttribute = new(
        "ZYL001",
        "Handler methods require [Controller] attribute on class",
        "Class '{0}' contains handler methods but is missing the [Controller] attribute. Add [Controller] to the class declaration.",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "Classes with [RequestHandler] or [EventHandler] methods must be marked with [Controller] attribute.");

    public static readonly DiagnosticDescriptor InvalidHandlerSignature = new(
        "ZYL002",
        "Invalid handler method signature",
        "Method '{0}' has invalid signature. {1}",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "Handler methods must follow the required signature patterns.");

    /// <summary>
    ///     Creates a diagnostic for an invalid handler method signature
    /// </summary>
    public static Diagnostic? CreateInvalidSignatureDiagnostic(IMethodSymbol method, string message)
    {
        var location = method.Locations.FirstOrDefault();
        if (location == null)
            return null;

        return Diagnostic.Create(
            InvalidHandlerSignature,
            location,
            method.Name,
            message);
    }

    /// <summary>
    ///     Creates a diagnostic for a class missing the [Controller] attribute
    /// </summary>
    public static Diagnostic? CreateMissingControllerAttributeDiagnostic(INamedTypeSymbol classSymbol)
    {
        var location = classSymbol.Locations.FirstOrDefault();
        if (location == null)
            return null;

        return Diagnostic.Create(
            MissingControllerAttribute,
            location,
            classSymbol.Name);
    }
}
