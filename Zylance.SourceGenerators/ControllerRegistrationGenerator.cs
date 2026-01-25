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
        // Find all classes that contain methods with RequestHandler or EventHandler attributes
        var controllerDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (s, _) => IsControllerClass(s),
                static (ctx, _) => GetControllerInfo(ctx))
            .Where(static m => m is not null);

        // Collect all controllers for assembly-wide registration
        var allControllers = controllerDeclarations.Collect();

        // Generate registration code for each controller
        context.RegisterSourceOutput(
            controllerDeclarations,
            static (spc, controller) => RouterServiceExtensionsCodeGenerator.Execute(controller!, spc));

        // Generate assembly-wide registration method
        context.RegisterSourceOutput(
            allControllers,
            static (spc, controllers) => AssemblyRegistrationCodeGenerator.Execute(controllers, spc));
    }

    private static bool IsControllerClass(SyntaxNode node)
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
                var result = AnalyzeRequestHandler(method, requestHandlerAttr);
                if (result.HandlerInfo != null)
                    handlers.Add(result.HandlerInfo);
                if (result.Diagnostic != null)
                    diagnostics.Add(result.Diagnostic);
            }
            else if (eventHandlerAttr != null)
            {
                var result = AnalyzeEventHandler(method, eventHandlerAttr);
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

    private static (HandlerInfo? HandlerInfo, Diagnostic? Diagnostic) AnalyzeRequestHandler(
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
            RequestTypeName = requestType.ToDisplayString(),
            ResponseTypeName = responseType.ToDisplayString(),
            IsAsync = isAsync,
        };

        return (handlerInfo, null);
    }

    private static (HandlerInfo? HandlerInfo, Diagnostic? Diagnostic) AnalyzeEventHandler(
        IMethodSymbol method,
        AttributeData attribute
    )
    {
        // Validate signature: Task(ZyEvent<TEvt>) or void(ZyEvent<TEvt>)
        if (method.Parameters.Length != 1)
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "EventHandler methods must have exactly 1 parameter");
            return (null, diagnostic);
        }

        var param = method.Parameters[0];

        if (!TypeChecks.IsZyEvent(param.Type))
        {
            var diagnostic = DiagnosticRules.CreateInvalidSignatureDiagnostic(
                method,
                "EventHandler methods must have signature: Task(ZyEvent<T>) or void(ZyEvent<T>)");
            return (null, diagnostic);
        }

        var eventType = ((INamedTypeSymbol)param.Type).TypeArguments[0];

        // Get event name from attribute or will be auto-detected
        string? eventName = null;
        if (attribute.ConstructorArguments.Length > 0)
            eventName = attribute.ConstructorArguments[0].Value as string;

        var isAsync = method.ReturnType.Name == "Task";

        var handlerInfo = new HandlerInfo
        {
            Type = HandlerType.Event,
            MethodName = method.Name,
            Action = eventName,
            EventTypeName = eventType.ToDisplayString(),
            IsAsync = isAsync,
        };

        return (handlerInfo, null);
    }
}
