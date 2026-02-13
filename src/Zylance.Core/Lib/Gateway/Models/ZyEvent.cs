using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Models;

/// <summary>
/// Represents an event received from the gateway, containing a payload and helpers to deserialize the event data.
/// </summary>
public class ZyEvent
{
    /// <summary>
    /// Raw event payload received from the gateway.
    /// </summary>
    public required EventPayload Payload { get; init; }

    /// <summary>
    /// Shortcut to the underlying event name.
    /// </summary>
    public string Name => Payload.EventName;

    /// <summary>
    /// Deserializes the event payload data into a protobuf message of type <typeparamref name="TData"/>.
    /// </summary>
    public TData GetData<TData>()
        where TData : IMessage, new()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
    }

    /// <summary>
    /// Attempts to deserialize the event payload into <typeparamref name="TData"/>, returning true on success.
    /// </summary>
    public bool TryGetData<TData>([NotNullWhen(true)] out TData? data)
        where TData : IMessage, new()
    {
        try
        {
            data = GetData<TData>();
            return true;
        }
        catch (Exception)
        {
            data = default;
            return false;
        }
    }
}

/// <summary>
/// Strongly-typed event wrapper for events with payload data of type <typeparamref name="TData"/>.
/// </summary>
public class ZyEvent<TData> : ZyEvent
    where TData : IMessage, new()
{
    /// <summary>
    /// Strongly-typed access to the event data of type <typeparamref name="TData"/>.
    /// </summary>
    public TData Data => GetData();

    /// <summary>
    /// Sets the event payload data from a strongly-typed protobuf message.
    /// </summary>
    public ZyEvent<TData> SetData(TData data)
    {
        Payload.DataJson = MessageUtils.ToJson(data);
        return this;
    }

    /// <summary>
    /// Deserializes and returns the strongly-typed event data.
    /// </summary>
    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
    }

    /// <summary>
    /// Attempts to get the strongly-typed event data, returning true on success.
    /// </summary>
    public bool TryGetData([NotNullWhen(true)] out TData? data)
    {
        try
        {
            data = GetData();
            return true;
        }
        catch (Exception)
        {
            data = default;
            return false;
        }
    }
}
