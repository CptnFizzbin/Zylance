using Serilog;
using Zylance.Contract.Api.Echo;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller exposing echo endpoints for diagnostics and testing.
/// </summary>
[Controller]
public class EchoController
{
    private static readonly ILogger Log = ZyLogger.ForContext<EchoController>();

    /// <summary>
    ///     Echoes the provided message back in the response.
    /// </summary>
    [RequestHandler]
    public void EchoMessage(ZyRequest<EchoReq> req, ZyResponse<EchoRes> res)
    {
        var message = req.GetData().Message;
        Log.Debug("EchoMessage called with Message={Message}", message);

        res.SetData(new EchoRes { Echoed = message });
        Log.Debug("EchoMessage responded with Echoed={Echoed}", message);
    }
}
