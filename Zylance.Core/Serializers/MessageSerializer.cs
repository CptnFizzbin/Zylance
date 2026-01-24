using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zylance.Core.Serializers;

public static class MessageSerializer
{
    private readonly static JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [RequiresUnreferencedCode("JSON serialization may require types that cannot be statically analyzed.")]
    public static string Serialize<TData>(TData data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    [RequiresUnreferencedCode("JSON serialization may require types that cannot be statically analyzed.")]
    public static TData? Deserialize<TData>(string message)
    {
        return JsonSerializer.Deserialize<TData>(message, Options);
    }
}
