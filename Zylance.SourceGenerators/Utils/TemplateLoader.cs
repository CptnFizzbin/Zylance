using System;
using System.IO;
using System.Reflection;
using HandlebarsDotNet;

namespace Zylance.SourceGenerators.Utils;

/// <summary>
///     Utility for loading embedded Handlebars templates
/// </summary>
internal static class TemplateLoader
{
    private static readonly Assembly Assembly = typeof(TemplateLoader).Assembly;
    private static readonly IHandlebars HandlebarsInstance;

    static TemplateLoader()
    {
        HandlebarsInstance = Handlebars.Create();
        
        // Register the 'eq' helper for equality comparisons
        HandlebarsInstance.RegisterHelper("eq", (_, arguments) =>
        {
            if (arguments.Length != 2) return false;
            return arguments[0]?.ToString() == arguments[1]?.ToString();
        });
    }

    public static HandlebarsTemplate<object, object> LoadTemplate(string templateName)
    {
        var resourceName = $"Zylance.SourceGenerators.Generators.{templateName}";

        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException(
                $"Could not find embedded template: {resourceName}. "
                + $"Available resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");

        using var reader = new StreamReader(stream);
        var templateSource = reader.ReadToEnd();
        
        return HandlebarsInstance.Compile(templateSource);
    }
}
