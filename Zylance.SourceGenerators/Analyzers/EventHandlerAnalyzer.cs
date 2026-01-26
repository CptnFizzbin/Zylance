using Microsoft.CodeAnalysis;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Analyzers;

/// <summary>
///     Analyzes event handler methods for validity and extracts handler information
/// </summary>
internal static class EventHandlerAnalyzer
{
    public static (HandlerInfo? HandlerInfo, Diagnostic? Diagnostic) Analyze(
        IMethodSymbol method,
        AttributeData attribute
    )
    {
        // Validate signature: Task(ZyEvent<TEvt>) or void(ZyEvent<TEvt>)
        if (method.Parameters.Length != 1)
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "EventHandler methods must have exactly 1 parameter");
            return (null, diagnostic);
        }

        var param = method.Parameters[0];

        if (!TypeChecks.IsZyEvent(param.Type))
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "EventHandler methods must have signature: Task(ZyEvent<T>) or void(ZyEvent<T>)");
            return (null, diagnostic);
        }

        var eventType = ((INamedTypeSymbol)param.Type).TypeArguments[0];

        // Get event name from attribute or will be auto-detected
        string? eventName = null;
        if (attribute.ConstructorArguments.Length > 0)
            eventName = attribute.ConstructorArguments[0].Value as string;

        var isAsync = method.ReturnType.Name == "Task";

        var handlerInfo = new HandlerInfo
        {
            Type = HandlerType.Event,
            MethodName = method.Name,
            Action = eventName,
            EventTypeName = eventType.Name,
            IsAsync = isAsync,
        };

        return (handlerInfo, null);
    }
}
