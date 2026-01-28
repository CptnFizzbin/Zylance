using Zylance.Contract.Api.Status;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.App.Controllers;

[Controller]
public class StatusController
{
    [RequestHandler]
    public void GetStatus(ZyRequest<GetStatusReq> req, ZyResponse<GetStatusRes> res)
    {
        res.SetData(
            new GetStatusRes
            {
                Status = "All systems operational",
            }
        );
    }
}
