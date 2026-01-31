using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Models;

public class ZyEvent
{
    public required EventPayload Payload { get; init; }

    public string Name => Payload.EventName;

    public TData GetData<TData>()
    {
        return MessageUtils.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize event data");
    }
}

public class ZyEvent<TData> : ZyEvent
{
    public TData Data => GetData();

    public ZyEvent<TData> SetData(TData data)
    {
        Payload.DataJson = MessageUtils.Serialize(data);
        return this;
    }

    public TData GetData()
    {
        return MessageUtils.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize response data");
    }
}
