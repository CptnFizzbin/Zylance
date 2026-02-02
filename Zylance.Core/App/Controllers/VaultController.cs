using Zylance.Contract.Api.Vault;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;
using Zylance.Core.Lib.Vault;

namespace Zylance.Core.App.Controllers;

[Controller]
public class VaultController(VaultService vaultService, VaultContext vaultContext)
{
    [RequestHandler]
    public async Task OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = await vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
    }

    [RequestHandler]
    public async Task CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = await vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
    }

    [RequestHandler]
    public void GetStatus(ZyRequest<VaultGetStatusReq> req, ZyResponse<VaultGetStatusRes> res)
    {
        var activeVault = vaultContext.ActiveVault;
        res.SetData(new VaultGetStatusRes { VaultRef = activeVault?.ToRef() });
    }
}
