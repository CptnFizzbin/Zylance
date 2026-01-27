using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Models;

internal record ControllerInfo
{
    public required INamedTypeSymbol ClassType { get; init; }
    public List<Diagnostic> Diagnostics { get; init; } = [];

    public required List<RequestHandlerInfo> RequestHandlers { get; init; }
    public required List<EventHandlerInfo> EventHandlers { get; init; }

    public int HandlerCount => EventHandlers.Count + RequestHandlers.Count;

    public HashSet<INamespaceSymbol> Namespaces =>
    [
        ClassType.ContainingNamespace,
        ..RequestHandlers.SelectMany(rh => rh.Namespaces),
        ..EventHandlers.SelectMany(eh => eh.Namespaces),
    ];

    public List<Diagnostic> AllDiagnostics =>
    [
        ..Diagnostics,
        ..RequestHandlers.SelectMany(rh => rh.Diagnostics),
        ..EventHandlers.SelectMany(eh => eh.Diagnostics),
    ];
}
