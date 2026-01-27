using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zylance.SourceGenerators.Analyzers;
using Zylance.SourceGenerators.Generators;
using Zylance.SourceGenerators.Models;

namespace Zylance.SourceGenerators;

/// <summary>
///     Source generator that discovers controller methods with [RequestHandler] and [EventHandler] attributes
///     and generates compile-time registration code, eliminating the need for runtime reflection.
/// </summary>
[Generator]
public class ZylanceSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var allControllers = FindControllers(context);

        context.RegisterSourceOutput(
            allControllers,
            static (spc, controller) => RouterServiceExtensionsCodeGenerator.Execute(controller, spc));

        context.RegisterSourceOutput(
            allControllers.Collect(),
            static (spc, controllers) =>
            {
                foreach (var diagnostic in controllers.SelectMany(controller => controller.AllDiagnostics))
                    spc.ReportDiagnostic(diagnostic);

                ControllerRegistrationCodeGenerator.Execute(controllers, spc);
            });
    }

    private static IncrementalValuesProvider<ControllerInfo> FindControllers(
        IncrementalGeneratorInitializationContext context
    )
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, cToken) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken: cToken);

                    if (symbol is not INamedTypeSymbol classSymbol) return null;

                    var isController = ControllerAnalyzer.IsController(classSymbol)
                        || ControllerAnalyzer.HasHandlers(classSymbol);
                    return isController
                        ? ControllerAnalyzer.Analyze(classSymbol)
                        : null;
                }
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);
    }
}
