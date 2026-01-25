using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Zylance.SourceGenerators.Models;

internal class ControllerInfo
{
    public string ClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string FullTypeName { get; set; } = "";
    public List<HandlerInfo> Handlers { get; set; } = new();
    public List<Diagnostic> Diagnostics { get; set; } = new();
}

internal class HandlerInfo
{
    public HandlerType Type { get; set; }
    public string MethodName { get; set; } = "";
    public string? Action { get; set; }
    public string? RequestTypeName { get; set; }
    public string? ResponseTypeName { get; set; }
    public string? EventTypeName { get; set; }
    public bool IsAsync { get; set; }
}

internal enum HandlerType
{
    Request,
    Event,
}
