using Microsoft.CodeAnalysis;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Analyzers;

/// <summary>
///     Analyzes request handler methods for validity and extracts handler information
/// </summary>
internal static class RequestHandlerAnalyzer
{
    public static (HandlerInfo? HandlerInfo, Diagnostic? Diagnostic) Analyze(
        IMethodSymbol method,
        AttributeData attribute
    )
    {
        // Validate signature: Task(ZyRequest<TReq>, ZyResponse<TRes>) or void(ZyRequest<TReq>, ZyResponse<TRes>)
        if (method.Parameters.Length != 2)
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "RequestHandler methods must have exactly 2 parameters");
            return (null, diagnostic);
        }

        var param1 = method.Parameters[0];
        var param2 = method.Parameters[1];

        if (!TypeChecks.IsZyRequest(param1.Type) || !TypeChecks.IsZyResponse(param2.Type))
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "RequestHandler methods must have signature: Task(ZyRequest<T>, ZyResponse<U>) or void(ZyRequest<T>, ZyResponse<U>)");
            return (null, diagnostic);
        }

        var requestType = ((INamedTypeSymbol)param1.Type).TypeArguments[0];
        var responseType = ((INamedTypeSymbol)param2.Type).TypeArguments[0];

        // Get action from attribute or will be auto-detected
        string? action = null;
        if (attribute.ConstructorArguments.Length > 0)
            action = attribute.ConstructorArguments[0].Value as string;

        var isAsync = method.ReturnType.Name == "Task";

        var handlerInfo = new HandlerInfo
        {
            Type = HandlerType.Request,
            MethodName = method.Name,
            Action = action,
            RequestTypeName = requestType.Name,
            ResponseTypeName = responseType.Name,
            IsAsync = isAsync,
        };

        return (handlerInfo, null);
    }
}
