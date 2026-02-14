using Zylance.Contract.Api.Echo;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Router.Attributes;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller exposing echo endpoints for diagnostics and testing.
/// </summary>
[Controller]
public class EchoController
{
    /// <summary>
    ///     Echoes the provided message back in the response.
    /// </summary>
    [RequestHandler]
    public void EchoMessage(ZyRequest<EchoReq> req, ZyResponse<EchoRes> res)
    {
        var message = req.GetData().Message;
        res.SetData(new EchoRes { Echoed = message });
    }
}
