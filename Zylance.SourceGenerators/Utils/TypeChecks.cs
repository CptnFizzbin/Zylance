using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Utils;

/// <summary>
///     Utility methods for checking Zylance types
/// </summary>
internal static class TypeChecks
{
    public static bool IsZyRequest(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named && named.Name == "ZyRequest" && named.TypeArguments.Length == 1;
    }

    public static bool IsZyResponse(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named && named.Name == "ZyResponse" && named.TypeArguments.Length == 1;
    }

    public static bool IsZyEvent(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named && named.Name == "ZyEvent" && named.TypeArguments.Length == 1;
    }
}
