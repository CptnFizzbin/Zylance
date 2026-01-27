using Microsoft.CodeAnalysis;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Roslyn;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Analyzers;

/// <summary>
///     Analyzes event handler methods for validity and extracts handler information
/// </summary>
internal static class EventHandlerAnalyzer
{
    public static EventHandlerInfo Analyze(IMethodSymbol method)
    {
        var handlerInfo = new EventHandlerInfo
        {
            Method = method,
            EventType = null,
            IsAsync = method.IsAsync,
        };

        if (method.Parameters.Length != 1)
        {
            handlerInfo.Diagnostics.Add(DiagnosticRules.CreateInvalidEventHandlerSignatureDiagnostic(method));
            return handlerInfo;
        }

        var param = method.Parameters[0];

        if (!TypeChecks.IsZyEvent(param.Type))
        {
            handlerInfo.Diagnostics.Add(DiagnosticRules.CreateInvalidEventHandlerSignatureDiagnostic(method));
            return handlerInfo;
        }

        return handlerInfo with
        {
            EventType = ((INamedTypeSymbol)param.Type).TypeArguments[0]
        };
    }
}
