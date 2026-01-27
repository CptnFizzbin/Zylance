using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Models;

internal record EventHandlerInfo
{
    public required IMethodSymbol Method { get; init; }
    public List<Diagnostic> Diagnostics { get; } = [];
    public ITypeSymbol? EventType { get; init; }
    public bool IsAsync { get; init; }

    public HashSet<INamespaceSymbol> Namespaces
    {
        get
        {
            HashSet<INamespaceSymbol> namespaces = [Method.ContainingNamespace];
            if (EventType?.ContainingNamespace != null)
                namespaces.Add(EventType.ContainingNamespace);
            return namespaces;
        }
    }

    public bool IsValid => EventType is not null;

    public ValidEventHandlerInfo ToValid()
    {
        if (!IsValid)
            throw new InvalidOperationException(
                "Cannot convert to ValidRequestHandlerInfo: RequestType or ResponseType is null.");

        return new ValidEventHandlerInfo
        {
            Method = Method,
            EventType = EventType!,
        };
    }
}

internal record ValidEventHandlerInfo : EventHandlerInfo
{
    public required new ITypeSymbol EventType { get; init; }
}
