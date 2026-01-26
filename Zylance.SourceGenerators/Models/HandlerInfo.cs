namespace Zylance.SourceGenerators.Models;

internal enum HandlerType
{
    Request,
    Event,
}

internal record HandlerInfo
{
    public HandlerType Type { get; init; }
    public string MethodName { get; init; } = "";
    public string? Action { get; init; }
    public string? RequestTypeName { get; init; }
    public string? ResponseTypeName { get; init; }
    public string? EventTypeName { get; init; }
    public bool IsAsync { get; init; }
}
