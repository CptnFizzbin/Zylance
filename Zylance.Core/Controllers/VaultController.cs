using Zylance.Contract.Api.Vault;
using Zylance.Core.Controllers.Attributes;
using Zylance.Core.Models;
using Zylance.Core.Services;

namespace Zylance.Core.Controllers;

[Controller]
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

    [EventHandler]
    private void OnVaultUpdated(ZyEvent<VaultOpenedEvt> evt)
    {
        Console.WriteLine($"[VaultController] Vault updated: {evt.Data.Vault.Id}");
    }
}
