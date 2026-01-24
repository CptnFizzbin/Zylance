using Zylance.Contract.Envelope;
using Zylance.Lib.Serializers;

namespace Zylance.Gateway.Handlers;

public interface IRequestHandler
{
    public bool IsRequestHandled(RequestPayload request);
    public ResponsePayload HandleRequest(RequestPayload request);

    public ResponsePayload RespondSuccess<TData>(string requestId, TData data)
    {
        return new ResponsePayload
        {
            RequestId = requestId,
            Status = "success",
            DataJson = MessageSerializer.Serialize(data),
        };
    }
}
