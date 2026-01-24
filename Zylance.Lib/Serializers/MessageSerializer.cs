using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zylance.Lib.Serializers;

public static class MessageSerializer
{
    private readonly static JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize<TData>(TData data)
    {
        return JsonSerializer.Serialize<TData>(data, Options);
    }

    public static TData? Deserialize<TData>(string message)
    {
        return JsonSerializer.Deserialize<TData>(message, Options);
    }
}
