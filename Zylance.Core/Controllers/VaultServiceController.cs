using Zylance.Contract.Messages.Vault;
using Zylance.Core.Attributes;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

[RequestController]
public class VaultController(VaultService vaultService)
{
    [RequestHandler("Vault:OpenVault")]
    private Task<ZyResponse<VaultOpenRes>> OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
        return Task.FromResult(res);
    }

    [RequestHandler("Vault:CreateVault")]
    private Task<ZyResponse<VaultCreateRes>> CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
        return Task.FromResult(res);
    }
}
