using Google.Protobuf;
using Zylance.Contract.Lib.Envelope;
using Zylance.Core.Gateway.Models;

namespace Zylance.Core.Tests.TestUtils.Factories.Models;

public static class ZyResponseTestFactory
{
    private static readonly Action<ZyResponse> DefaultOnSend = _ => { };

    public static ZyResponse<TData> Create<TData>(Action<ZyResponse>? onSend = null)
        where TData : IMessage, new()
    {
        return new ZyResponse<TData> { Payload = new ResponsePayload(), OnSend = onSend ?? DefaultOnSend };
    }
}
