using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Models;

public class ZyRequest
{
    public required RequestPayload Payload { get; init; }
    public string Action => Payload.Action;

    public TData GetData<TData>()
        where TData : IMessage, new()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }

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

public class ZyRequest<TData> : ZyRequest
    where TData : IMessage, new()
{
    public TData Data => GetData();

    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }

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
