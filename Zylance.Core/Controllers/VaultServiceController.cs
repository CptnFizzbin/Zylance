using Zylance.Contract.Api.Vault;
using Zylance.Core.Attributes;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

[RequestController]
public class VaultController(VaultService vaultService)
{
    [RequestHandler]
    private void OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
    }

    [RequestHandler]
    private void CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
    }
}
