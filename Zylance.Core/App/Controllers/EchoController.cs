using Zylance.Contract.Api.Echo;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.App.Controllers;

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
