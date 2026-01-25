using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Zylance.SourceGenerators.Models;
using Zylance.SourceGenerators.Utils;

namespace Zylance.SourceGenerators.Generators;

/// <summary>
///     Generates assembly-wide registration code for all controllers
/// </summary>
internal static class AssemblyRegistrationCodeGenerator
{
    private static readonly HandlebarsDotNet.HandlebarsTemplate<object, object> Template =
        TemplateLoader.LoadTemplate("ControllerRegistration.cs.hbs");

    public static void Execute(
        ImmutableArray<ControllerInfo?> controllers,
        SourceProductionContext context)
    {
        var validControllers = controllers.Where(c => c != null).Cast<ControllerInfo>().ToList();

        if (validControllers.Count == 0)
            return;

        var source = Generate(validControllers);
        context.AddSource("ControllerRegistration.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string Generate(List<ControllerInfo> controllers)
    {
        var primaryNamespace = controllers
            .GroupBy(c => c.Namespace)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

        var model = new
        {
            @namespace = primaryNamespace,
            controller_count = controllers.Count,
            controllers = controllers.OrderBy(c => c.ClassName).Select(c => new
            {
                class_name = c.ClassName,
                full_type_name = c.FullTypeName,
                handler_count = c.Handlers.Count,
                camel_case_name = ToCamelCase(c.ClassName),
            }).ToList(),
        };

        return Template(model);
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
