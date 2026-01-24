using Zylance.Core.Interfaces;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers;

public class StatusController
{
    private const string Name = "Status";
    private readonly RequestRouter _router;

    public StatusController()
    {
        _router = new RequestRouter()
            .Use($"{Name}:GetStatus", GetStatus);
    }

    public Task<ZyResponse> HandleRequest(ZyRequest req, ZyResponse res)
    {
        return _router.MessageReceived(req, res);
    }

    private Task<ZyResponse> GetStatus(ZyRequest req, ZyResponse res)
    {
        // Since there's no typed response in the protobuf, we'll just set status
        res.SetSatus("OK");
        return Task.FromResult(res);
    }
}
