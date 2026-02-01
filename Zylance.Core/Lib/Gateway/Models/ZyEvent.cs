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
            ?? throw new ArgumentException("Failed to deserialize response data");
    }
}
