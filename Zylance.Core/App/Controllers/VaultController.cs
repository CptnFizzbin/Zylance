using Zylance.Contract.Api.Vault;
using Zylance.Core.App.Services;
using Zylance.Core.Lib.Gateway.Attributes;
using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.App.Controllers;

[Controller]
public class VaultController(VaultService vaultService)
{
    [RequestHandler]
    public void OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
    }

    [RequestHandler]
    public void CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
    }

    [EventHandler]
    public void OnVaultUpdated(ZyEvent<VaultOpenedEvt> evt)
    {
        Console.WriteLine($"[VaultController] Vault updated: {evt.Data.Vault.Id}");
    }
}
