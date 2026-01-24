using Zylance.Contract.Envelope;
using Zylance.Core.Serializers;

namespace Zylance.Core.Models;

public class ZyResponse
{
    public required ResponsePayload Payload { get; init; }
    public string Status => Payload.Status;

    public ZyResponse SetStatus(string status)
    {
        Payload.Status = status;
        return this;
    }

    public ZyResponse SetData<TData>(TData data)
    {
        Payload.DataJson = MessageSerializer.Serialize(data);
        return this;
    }
}

public class ZyResponse<TData> : ZyResponse
{
    public ZyResponse<TData> SetData(TData data)
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
