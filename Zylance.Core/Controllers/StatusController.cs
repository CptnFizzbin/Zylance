using Zylance.Contract.Messages.Status;
using Zylance.Core.Attributes;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers;

[RequestController]
public class StatusController
{
    [RequestHandler]
    private Task<ZyResponse<GetStatusRes>> GetStatus(ZyRequest<GetStatusReq> req, ZyResponse<GetStatusRes> res)
    {
        // Since there's no typed response in the protobuf, we'll just set status
        res.SetStatus("OK");
        return Task.FromResult(res);
    }
}
