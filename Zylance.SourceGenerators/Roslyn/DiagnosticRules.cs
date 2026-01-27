using System.Linq;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Roslyn;

/// <summary>
///     Diagnostic rules for controller registration source generator
/// </summary>
internal static class DiagnosticRules
{
    private readonly static DiagnosticDescriptor MissingControllerAttribute = new(
        "ZYL001",
        "Handler methods require [Controller] attribute on class",
        "Class '{0}' contains handler methods but is missing the [Controller] attribute. Add [Controller] to the class declaration.",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "Classes with [RequestHandler] or [EventHandler] methods must be marked with [Controller] attribute.");

    private readonly static DiagnosticDescriptor InvalidRequestHandlerSignature = new(
        "ZYL002",
        "Invalid handler method signature",
        "Method '{0}' has invalid signature. RequestHandler methods must have signature: Task(ZyRequest<Req>, ZyResponse<Res>) or void(ZyRequest<Req>, ZyResponse<Res>).",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "Handler methods must follow the required signature patterns.");

    private readonly static DiagnosticDescriptor InvalidEventHandlerSignature = new(
        "ZYL003",
        "Invalid handler method signature",
        "Method '{0}' has invalid signature. EventHandler methods must have signature: Task(ZyEvent<T>) or void(ZyEvent<T>).",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "Handler methods must follow the required signature patterns.");

    private readonly static DiagnosticDescriptor RequestHandlerTypeMismatch = new(
        "ZYL004",
        "Request and Response type mismatch",
        "Method '{0}' has invalid signature. The Request and Response Data Types must be for the same Request.",
        "Zylance.Controllers",
        DiagnosticSeverity.Error,
        true,
        "The request and response types in handler method signatures must match.");

    /// <summary>
    ///     Creates a diagnostic for a class missing the [Controller] attribute
    /// </summary>
    public static Diagnostic CreateMissingControllerAttributeDiagnostic(INamedTypeSymbol classSymbol)
    {
        return Diagnostic.Create(
            MissingControllerAttribute,
            classSymbol.Locations.First(),
            classSymbol.Name);
    }

    /// <summary>
    ///     Creates a diagnostic for an invalid handler method signature
    /// </summary>
    public static Diagnostic CreateInvalidRequestHandlerSignatureDiagnostic(IMethodSymbol method)
    {
        return Diagnostic.Create(
            InvalidRequestHandlerSignature,
            method.Locations.First(),
            method.Name);
    }

    /// <summary>
    ///     Creates a diagnostic for an invalid handler method signature
    /// </summary>
    public static Diagnostic CreateInvalidEventHandlerSignatureDiagnostic(IMethodSymbol method)
    {
        return Diagnostic.Create(
            InvalidEventHandlerSignature,
            method.Locations.First(),
            method.Name);
    }

    /// <summary>
    ///     Creates a diagnostic for an invalid handler method signature
    /// </summary>
    public static Diagnostic CreateRequestHandlerTypeMismatchDiagnostic(IMethodSymbol method)
    {
        return Diagnostic.Create(
            RequestHandlerTypeMismatch,
            method.Locations.First(),
            method.Name);
    }
}
