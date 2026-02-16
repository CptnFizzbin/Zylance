using Serilog;
using Zylance.Contract.Api.Status;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller that exposes runtime status endpoints for health checks.
/// </summary>
[Controller]
public class StatusController
{
    private static readonly ILogger Log = ZyLogger.ForContext<StatusController>();

    /// <summary>
    ///     Returns a basic status response for health monitoring.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to populate with status info.</param>
    [RequestHandler]
    public void GetStatus(ZyRequest<GetStatusReq> req, ZyResponse<GetStatusRes> res)
    {
        Log.Debug("GetStatus called");
        res.SetData(new GetStatusRes { Status = "All systems operational" });
        Log.Debug("GetStatus responded");
    }
}
