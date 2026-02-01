using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;

namespace Zylance.Core.Lib.Gateway.Utils;

public static class MessageUtils
{
    private static readonly JsonFormatter Formatter = new(JsonFormatter.Settings.Default);
    private static readonly JsonParser Parser = new(JsonParser.Settings.Default);

    public static string ToJson<TData>(TData data)
        where TData : IMessage
    {
        return Formatter.Format(data);
    }

    public static TData? FromJson<TData>(string message)
        where TData : IMessage, new()
    {
        return Parser.Parse<TData>(message);
    }

    public static bool IsEvent(GatewayEnvelope message)
    {
        return message.Event != null;
    }

    public static bool IsEventWithPrefix(GatewayEnvelope message, string prefix)
    {
        return IsEvent(message) && message.Event.EventName.StartsWith(prefix);
    }

    public static RequestPayload ToRequestPayload<TData>(Guid? requestId, TData? data)
        where TData : IMessage, new()
    {
        var payload = new RequestPayload
        {
            RequestId = (requestId ?? Guid.CreateVersion7()).ToString(),
            Action = ProtoActionUtils.GetAction<TData>(),
            DataJson = data is not null ? ToJson(data) : null,
        };

        return payload;
    }

    public static ResponsePayload ToResponsePayload<TData>(Guid requestId, string? status, TData? data)
        where TData : IMessage, new()
    {
        var payload = new ResponsePayload
        {
            RequestId = requestId.ToString(),
            Status = status ?? "Success",
            DataJson = data is not null ? ToJson(data) : null,
        };

        return payload;
    }

    public static EventPayload ToEventPayload<TData>(TData data)
        where TData : IMessage, new()
    {
        var payload = new EventPayload { EventName = ProtoActionUtils.GetEventName(data), DataJson = ToJson(data) };

        return payload;
    }
}
