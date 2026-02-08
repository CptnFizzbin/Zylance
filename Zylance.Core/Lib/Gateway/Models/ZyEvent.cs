using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Models;

public class ZyEvent
{
    public required EventPayload Payload { get; init; }

    public string Name => Payload.EventName;

    public TData GetData<TData>()
        where TData : IMessage, new()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
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

public class ZyEvent<TData> : ZyEvent
    where TData : IMessage, new()
{
    public TData Data => GetData();

    public ZyEvent<TData> SetData(TData data)
    {
        Payload.DataJson = MessageUtils.ToJson(data);
        return this;
    }

    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
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
