using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Gateway.Utils;

namespace Zylance.Core.Tests.TestUtils.Factories;

public static class ZyRequestTestFactory
{
    public static ZyRequest<TData> Create<TData>(TData data)
        where TData : IMessage, new()
    {
        return new ZyRequest<TData>
        {
            Payload = new RequestPayload
            {
                Action = ProtoActionUtils.GetAction<TData>(),
                DataJson = MessageUtils.ToJson(data),
            },
        };
    }
}
