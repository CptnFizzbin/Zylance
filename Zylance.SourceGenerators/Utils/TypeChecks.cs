using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    public static bool IsControllerClass(SyntaxNode node)
    {
        // Look for classes marked with [Controller] attribute
        if (node is not ClassDeclarationSyntax classDecl)
            return false;

        return classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Any(attr =>
            {
                var name = attr.Name.ToString();
                return name == "Controller" || name == "ControllerAttribute";
            });
    }
}
