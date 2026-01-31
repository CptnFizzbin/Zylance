using System.Text.Json;
using System.Text.Json.Serialization;
using Zylance.Contract.Lib.Envelope;

namespace Zylance.Core.Lib.Gateway.Utils;

public static class MessageUtils
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize<TData>(TData data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public static TData? Deserialize<TData>(string message)
    {
        return JsonSerializer.Deserialize<TData>(message, Options);
    }

    public static bool IsEvent(GatewayEnvelope message)
    {
        return message.Event != null;
    }

    public static bool IsEventWithPrefix(GatewayEnvelope message, string prefix)
    {
        return IsEvent(message) && message.Event.EventName.StartsWith(prefix);
    }
}
