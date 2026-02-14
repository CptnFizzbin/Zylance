using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Utils;

namespace Zylance.Core.Gateway.Models;

/// <summary>
///     Represents a response message sent via the gateway with helpers to set and
///     get typed data.
/// </summary>
public class ZyResponse
{
    /// <summary>
    ///     Raw response payload that will be sent over the gateway.
    /// </summary>
    public required ResponsePayload Payload { get; init; }

    /// <summary>
    ///     Short-hand for the response status string.
    /// </summary>
    public string Status => Payload.Status;

    /// <summary>
    ///     Sets the response status string.
    /// </summary>
    public ZyResponse SetStatus(string status)
    {
        Payload.Status = status;
        return this;
    }

    /// <summary>
    ///     Sets the response payload data from a protobuf message.
    /// </summary>
    public ZyResponse SetData<TData>(TData data)
        where TData : IMessage
    {
        Payload.DataJson = MessageUtils.ToJson(data);
        return this;
    }

    /// <summary>
    ///     Deserializes the response payload data into a protobuf message of type
    ///     <typeparamref name="TData" />.
    /// </summary>
    public TData GetData<TData>()
        where TData : IMessage, new()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize response data");
    }

    /// <summary>
    ///     Attempts to deserialize the response payload into
    ///     <typeparamref name="TData" />, returning true on success.
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
///     Strongly-typed response wrapper for responses carrying data of type
///     <typeparamref name="TData" />.
/// </summary>
public class ZyResponse<TData> : ZyResponse
    where TData : IMessage, new()
{
    /// <summary>
    ///     Sets the strongly-typed response data.
    /// </summary>
    public ZyResponse<TData> SetData(TData data)
    {
        Payload.DataJson = MessageUtils.ToJson(data);
        return this;
    }

    /// <summary>
    ///     Deserializes and returns the strongly-typed response data.
    /// </summary>
    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize response data");
    }

    /// <summary>
    ///     Attempts to get the strongly-typed response data, returning true on
    ///     success.
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
