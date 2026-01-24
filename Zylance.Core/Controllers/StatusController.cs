using Zylance.Core.Attributes;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers;

[RequestController]
public class StatusController
{
    [RequestHandler("Status:GetStatus")]
    private Task<ZyResponse> GetStatus(ZyRequest req, ZyResponse res)
    {
        // Since there's no typed response in the protobuf, we'll just set status
        res.SetStatus("OK");
        return Task.FromResult(res);
    }
}
