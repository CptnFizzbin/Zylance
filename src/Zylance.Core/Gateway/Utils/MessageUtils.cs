using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;

namespace Zylance.Core.Gateway.Utils;

/// <summary>
///     Helper methods for serializing and deserializing gateway payloads to/from
///     JSON.
/// </summary>
public static class MessageUtils
{
    private static readonly JsonFormatter Formatter = new(JsonFormatter.Settings.Default);
    private static readonly JsonParser Parser = new(JsonParser.Settings.Default);

    /// <summary>
    ///     Serializes a protobuf message to JSON using the configured formatter.
    /// </summary>
    public static string ToJson<TData>(TData data)
        where TData : IMessage
    {
        return Formatter.Format(data);
    }

    /// <summary>
    ///     Deserializes a JSON message into a protobuf message instance of type
    ///     <typeparamref name="TData" />.
    /// </summary>
    public static TData? FromJson<TData>(string message)
        where TData : IMessage, new()
    {
        return Parser.Parse<TData>(message);
    }

    /// <summary>
    ///     Returns true if the specified envelope contains an event payload.
    /// </summary>
    public static bool IsEvent(GatewayEnvelope message)
    {
        return message.Event != null;
    }

    /// <summary>
    ///     Returns true if the envelope contains an event whose name starts with the
    ///     provided prefix.
    /// </summary>
    public static bool IsEventWithPrefix(GatewayEnvelope message, string prefix)
    {
        return IsEvent(message) && message.Event.EventName.StartsWith(prefix);
    }

    /// <summary>
    ///     Creates a RequestPayload for the specified data using the protobuf action
    ///     option.
    /// </summary>
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

    /// <summary>
    ///     Creates a ResponsePayload for the specified data and request id.
    /// </summary>
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

    /// <summary>
    ///     Creates an EventPayload from a protobuf message using its event name
    ///     option.
    /// </summary>
    public static EventPayload ToEventPayload<TData>(TData data)
        where TData : IMessage, new()
    {
        var payload = new EventPayload { EventName = ProtoActionUtils.GetEventName(data), DataJson = ToJson(data) };

        return payload;
    }
}
