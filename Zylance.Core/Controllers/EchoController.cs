using Zylance.Contract.Api.Echo;
using Zylance.Core.Controllers.Attributes;
using Zylance.Core.Controllers.Models;

namespace Zylance.Core.Controllers;

[Controller]
public class EchoController
{
    [RequestHandler]
    public void EchoMessage(ZyRequest<EchoReq> req, ZyResponse<EchoRes> res)
    {
        var message = req.GetData().Message;
        res.SetData(new EchoRes { Echoed = message });
    }
}
