using Microsoft.CodeAnalysis;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Roslyn;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Analyzers;

/// <summary>
///     Analyzes request handler methods for validity and extracts handler information
/// </summary>
internal static class RequestHandlerAnalyzer
{
    public static RequestHandlerInfo Analyze(IMethodSymbol method)
    {
        var handlerInfo = new RequestHandlerInfo
        {
            Method = method,
            RequestType = null,
            ResponseType = null,
            IsAsync = method.IsAsync,
        };

        if (method.Parameters.Length != 2)
        {
            handlerInfo.Diagnostics.Add(DiagnosticRules.CreateInvalidRequestHandlerSignatureDiagnostic(method));
            return handlerInfo;
        }

        var param1 = method.Parameters[0];
        var param2 = method.Parameters[1];

        if (!TypeChecks.IsZyRequest(param1.Type) || !TypeChecks.IsZyResponse(param2.Type))
        {
            handlerInfo.Diagnostics.Add(DiagnosticRules.CreateInvalidRequestHandlerSignatureDiagnostic(method));
            return handlerInfo;
        }

        var requestType = ((INamedTypeSymbol)param1.Type).TypeArguments[0];
        var requestMessageName = requestType.Name.Replace("Req", "");

        var responseType = ((INamedTypeSymbol)param2.Type).TypeArguments[0];
        var responseMessageName = responseType.Name.Replace("Res", "");

        if (requestMessageName != responseMessageName)
        {
            handlerInfo.Diagnostics.Add(DiagnosticRules.CreateRequestHandlerTypeMismatchDiagnostic(method));
        }

        return handlerInfo with
        {
            RequestType = requestType,
            ResponseType = responseType,
        };
    }
}
