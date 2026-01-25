using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Generators;

/// <summary>
///     Generates registration code for individual controllers
/// </summary>
internal static class RouterServiceExtensionsCodeGenerator
{
    private static readonly HandlebarsDotNet.HandlebarsTemplate<object, object> Template =
        TemplateLoader.LoadTemplate("RouterServiceExtensions.cs.hbs");

    public static void Execute(ControllerInfo controller, SourceProductionContext context)
    {
        // Report any diagnostics
        foreach (var diagnostic in controller.Diagnostics)
            context.ReportDiagnostic(diagnostic);

        // Only generate code if there are handlers
        if (controller.Handlers.Count <= 0)
            return;

        var source = Generate(controller);
        context.AddSource($"{controller.ClassName}_Registration.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string Generate(ControllerInfo controller)
    {
        var model = new
        {
            @namespace = controller.Namespace,
            class_name = controller.ClassName,
            full_type_name = controller.FullTypeName,
            handlers = controller.Handlers.Select(h => new
            {
                type = h.Type.ToString(),
                method_name = h.MethodName,
                action = h.Action,
                request_type = h.RequestTypeName,
                response_type = h.ResponseTypeName,
                event_type = h.EventTypeName,
                is_async = h.IsAsync,
            }).ToList(),
        };

        return Template(model);
    }
}
