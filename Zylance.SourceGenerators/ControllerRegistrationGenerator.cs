using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Zylance.SourceGenerators.Analyzers;
using Zylance.SourceGenerators.Generators;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators;

/// <summary>
///     Source generator that discovers controller methods with [RequestHandler] and [EventHandler] attributes
///     and generates compile-time registration code, eliminating the need for runtime reflection.
/// </summary>
[Generator]
public class ControllerRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controllerDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => TypeChecks.IsControllerClass(s),
                static (ctx, _) => GetControllerInfo(ctx))
            .Where(static m => m is not null);

        var allControllers = controllerDeclarations.Collect();

        context.RegisterSourceOutput(
            controllerDeclarations,
            static (spc, controller) => RouterServiceExtensionsCodeGenerator.Execute(controller!, spc));

        context.RegisterSourceOutput(
            allControllers,
            static (spc, controllers) => AssemblyRegistrationCodeGenerator.Execute(controllers, spc));
    }


    private static ControllerInfo? GetControllerInfo(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);

        if (symbol is not INamedTypeSymbol classSymbol)
            return null;

        var diagnostics = new List<Diagnostic>();

        // Verify the class has [Controller] attribute
        var hasControllerAttribute = classSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "ControllerAttribute");

        if (!hasControllerAttribute)
        {
            // Check if there are any handler methods without [Controller] attribute
            var hasHandlerMethods = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .Any(m => m.GetAttributes().Any(a =>
                    a.AttributeClass?.Name == "RequestHandlerAttribute"
                    || a.AttributeClass?.Name == "EventHandlerAttribute"));

            if (hasHandlerMethods)
            {
                // Create diagnostic: handlers found but no [Controller] attribute
                var diagnostic = DiagnosticRules.CreateMissingControllerAttributeDiagnostic(classSymbol);
                if (diagnostic != null)
                    diagnostics.Add(diagnostic);
            }

            return null;
        }

        var handlers = new List<HandlerInfo>();

        foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            var requestHandlerAttr = method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "RequestHandlerAttribute");

            var eventHandlerAttr = method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "EventHandlerAttribute");

            if (requestHandlerAttr != null)
            {
                var result = RequestHandlerAnalyzer.Analyze(method, requestHandlerAttr);
                if (result.HandlerInfo != null)
                    handlers.Add(result.HandlerInfo);
                if (result.Diagnostic != null)
                    diagnostics.Add(result.Diagnostic);
            }
            else if (eventHandlerAttr != null)
            {
                var result = EventHandlerAnalyzer.Analyze(method, eventHandlerAttr);
                if (result.HandlerInfo != null)
                    handlers.Add(result.HandlerInfo);
                if (result.Diagnostic != null)
                    diagnostics.Add(result.Diagnostic);
            }
        }

        if (handlers.Count == 0 && diagnostics.Count == 0)
            return null;

        return new ControllerInfo
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            FullTypeName = classSymbol.ToDisplayString(),
            Handlers = handlers,
            Diagnostics = diagnostics,
        };
    }
}
