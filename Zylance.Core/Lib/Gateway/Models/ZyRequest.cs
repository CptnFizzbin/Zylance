using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Lib.Gateway.Utils;

namespace Zylance.Core.Lib.Gateway.Models;

public class ZyRequest
{
    public required RequestPayload Payload { get; init; }
    public string Action => Payload.Action;

    public TData GetData<TData>()
    {
        return MessageSerializer.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }
}

public class ZyRequest<TData> : ZyRequest
{
    public TData GetData()
    {
        return MessageSerializer.Deserialize<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }
}
