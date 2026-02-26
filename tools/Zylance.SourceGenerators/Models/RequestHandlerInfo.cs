using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Models;

internal record RequestHandlerInfo
{
    public required IMethodSymbol Method { get; init; }
    public List<Diagnostic> Diagnostics { get; } = [];
    public ITypeSymbol? RequestType { get; init; }
    public ITypeSymbol? ResponseType { get; init; }

    public bool IsAsync => Method.IsAsync || Method.ReturnType.Name == "Task";

    public HashSet<INamespaceSymbol> Namespaces
    {
        get
        {
            HashSet<INamespaceSymbol> namespaces = [Method.ContainingNamespace];
            if (RequestType?.ContainingNamespace != null)
                namespaces.Add(RequestType.ContainingNamespace);
            if (ResponseType?.ContainingNamespace != null)
                namespaces.Add(ResponseType.ContainingNamespace);
            return namespaces;
        }
    }

    public bool IsValid => RequestType is not null && ResponseType is not null;

    public ValidRequestHandlerInfo ToValid()
    {
        if (!IsValid)
            throw new InvalidOperationException(
                "Cannot convert to ValidRequestHandlerInfo: RequestType or ResponseType is null."
            );

        return new ValidRequestHandlerInfo
        {
            Method = Method,
            RequestType = RequestType!,
            ResponseType = ResponseType!,
        };
    }
}

internal record ValidRequestHandlerInfo : RequestHandlerInfo
{
    public new required ITypeSymbol RequestType { get; init; }
    public new required ITypeSymbol ResponseType { get; init; }
}
