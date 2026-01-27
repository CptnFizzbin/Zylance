using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Roslyn;

namespace Zylance.SourceGenerators.Analyzers;

public static class ControllerAnalyzer
{
    internal static bool IsController(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "ControllerAttribute");
    }

    internal static bool HasHandlers(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .ToList()
            .SelectMany(m => m.GetAttributes())
            .Any(a => a.AttributeClass?.Name is "RequestHandlerAttribute" or "EventHandlerAttribute");
    }

    internal static ControllerInfo Analyze(INamedTypeSymbol classSymbol)
    {
        var diagnostics = new List<Diagnostic>();

        var requestHandlers = new List<RequestHandlerInfo>();
        var eventHandlers = new List<EventHandlerInfo>();

        var allMethods = classSymbol.GetMembers().OfType<IMethodSymbol>().ToList();

        var reqHandlerMethods = allMethods
            .Where(m => m.GetAttributes()
                .Any(a => a.AttributeClass?.Name == "RequestHandlerAttribute"))
            .ToList();

        var eventHandlerMethods = allMethods
            .Where(m => m.GetAttributes()
                .Any(a => a.AttributeClass?.Name == "EventHandlerAttribute"))
            .ToList();

        reqHandlerMethods.ForEach(method => requestHandlers.Add(RequestHandlerAnalyzer.Analyze(method)));
        eventHandlerMethods.ForEach(method => eventHandlers.Add(EventHandlerAnalyzer.Analyze(method)));

        var numHandlers = eventHandlers.Count + requestHandlers.Count;
        if (!IsController(classSymbol) && numHandlers != 0)
            diagnostics.Add(DiagnosticRules.CreateMissingControllerAttributeDiagnostic(classSymbol));

        return new ControllerInfo
        {
            ClassType = classSymbol,
            Diagnostics = diagnostics,
            RequestHandlers = requestHandlers,
            EventHandlers = eventHandlers,
        };
    }
}
