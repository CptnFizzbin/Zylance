using Zylance.Contract.Messages.Vault;
using Zylance.Core.Interfaces;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

public class VaultController
{
    private const string Name = "Vault";
    private readonly RequestRouter _router;

    private readonly VaultService _vaultService;

    public VaultController(VaultService vaultService)
    {
        _vaultService = vaultService;
        _router = new RequestRouter()
            .Use<VaultOpenReq, VaultOpenRes>($"{Name}:OpenVault", OpenVault)
            .Use<VaultCreateReq, VaultCreateRes>($"{Name}:CreateVault", CreateVault);
    }

    public Task<ZyResponse> HandleRequest(ZyRequest zyRequest, ZyResponse zyResponse)
    {
        return _router.MessageReceived(zyRequest, zyResponse);
    }

    private Task<ZyResponse<VaultOpenRes>> OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = _vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
        return Task.FromResult(res);
    }

    private Task<ZyResponse<VaultCreateRes>> CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = _vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
        return Task.FromResult(res);
    }
}
