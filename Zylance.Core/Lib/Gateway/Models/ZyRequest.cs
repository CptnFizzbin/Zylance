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
}

public class ZyRequest<TData> : ZyRequest
    where TData : IMessage, new()
{
    public TData GetData()
    {
        return MessageUtils.FromJson<TData>(Payload.DataJson)
            ?? throw new ArgumentException("Failed to deserialize request data");
    }
}
