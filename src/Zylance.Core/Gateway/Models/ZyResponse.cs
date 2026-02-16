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
    ///     Callback invoked when the response is sent. Implementations must set
    ///     this before calling <see cref="Send" /> to perform the actual send
    ///     operation (for example, serializing and writing to the transport).
    /// </summary>
    public required Action<ZyResponse> OnSend { get; init; }

    /// <summary>
    ///     Raw response payload that will be sent over the gateway.
    /// </summary>
    public required ResponsePayload Payload { get; init; }

    /// <summary>
    ///     Shorthand for the response status string.
    /// </summary>
    public string Status => Payload.Status;

    /// <summary>
    ///     True once <see cref="Send" /> has been called for this response. Used to
    ///     ensure the response is only sent once.
    /// </summary>
    public bool ResponseSent { get; private set; }

    /// <summary>
    ///     Sets the response status string.
    /// </summary>
    public ZyResponse SetStatus(string status)
    {
        Payload.Status = status;
        return this;
    }

    /// <summary>
    ///     Marks the response as sent and invokes the configured <see cref="OnSend" />
    ///     callback.
    ///     Subsequent calls are ignored.
    /// </summary>
    public void Send()
    {
        if (ResponseSent)
            return;

        ResponseSent = true;
        OnSend(this);
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
