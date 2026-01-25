using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Serializers;

namespace Zylance.Core.Models;

public class ZyEvent
{
    public required EventPayload Payload { get; init; }

    public string Name => Payload.Event;

    public TData GetData<TData>()
    {
        return MessageSerializer.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
    }
}

public class ZyEvent<TData> : ZyEvent
{
    public TData Data => GetData();

    public ZyEvent<TData> SetData(TData data)
    {
        Payload.DataJson = MessageSerializer.Serialize(data);
        return this;
    }

    public TData GetData()
    {
        return MessageSerializer.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize response data");
    }
}
