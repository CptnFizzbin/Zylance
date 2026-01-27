using Zylance.Contract.Api.Status;
using Zylance.Core.Controllers.Attributes;
using Zylance.Core.Controllers.Models;

namespace Zylance.Core.Controllers;

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
