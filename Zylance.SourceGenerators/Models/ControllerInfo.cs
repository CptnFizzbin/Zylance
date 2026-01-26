using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Models;

internal record ControllerInfo
{
    public string ClassName { get; init; } = "";
    public string Namespace { get; init; } = "";
    public string FullTypeName { get; init; } = "";
    public List<HandlerInfo> Handlers { get; init; } = new();
    public List<Diagnostic> Diagnostics { get; init; } = new();
}
