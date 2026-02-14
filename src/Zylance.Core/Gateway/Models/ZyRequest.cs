using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Utils;

namespace Zylance.Core.Gateway.Models;

/// <summary>
///     Represents a request message received via the gateway along with helpers to
///     access typed payload data.
/// </summary>
public class ZyRequest
{
    /// <summary>
    ///     Raw request payload received from the gateway.
    /// </summary>
    public required RequestPayload Payload { get; init; }

    /// <summary>
    ///     Shortcut to the request action name.
    /// </summary>
    public string Action => Payload.Action;

    /// <summary>
    ///     Deserializes the request payload data into a protobuf message of type
    ///     <typeparamref name="TData" />.
    /// </summary>
    public TData GetData<TData>()
        where TData : IMessage, new()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }

    /// <summary>
    ///     Attempts to deserialize the request payload into
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
///     Strongly-typed request wrapper for requests carrying data of type
///     <typeparamref name="TData" />.
/// </summary>
public class ZyRequest<TData> : ZyRequest
    where TData : IMessage, new()
{
    /// <summary>
    ///     Strongly-typed access to the request data.
    /// </summary>
    public TData Data => GetData();

    /// <summary>
    ///     Deserializes and returns the strongly-typed request data.
    /// </summary>
    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }

    /// <summary>
    ///     Attempts to get the strongly-typed request data, returning true on success.
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
