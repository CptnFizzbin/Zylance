using Zylance.Contract.Messages.Echo;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;

namespace Zylance.Core.Controllers;

public class EchoController
{
    private const string Name = "Echo";
    private readonly RequestRouter _router;

    public EchoController()
    {
        _router = new RequestRouter()
            .Use<EchoReq, EchoRes>($"{Name}:EchoMessage", EchoMessage);
    }

    public Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        return _router.MessageReceived(zyRequest, zyResponse);
    }

    private Task<ZyResponse<EchoRes>> EchoMessage(ZyRequest<EchoReq> req, ZyResponse<EchoRes> res)
    {
        var message = req.GetData().Message;
        res.SetData(new EchoRes { Echoed = message });
        return Task.FromResult(res);
    }
}
