using Zylance.Contract.Api.Vault;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Router.Attributes;
using Zylance.Core.Vault.Services;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller exposing vault lifecycle operations (open/create/close/status).
/// </summary>
[Controller]
public class VaultController(VaultService vaultService)
{
    /// <summary>
    ///     Opens the active vault and returns its reference.
    /// </summary>
    /// <param name="req">Request object</param>
    /// <param name="res">Response to populate with the opened vault reference.</param>
    [RequestHandler]
    public async Task OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        var vaultRef = await vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
    }

    /// <summary>
    ///     Creates a new vault and sets it as active, returning the vault reference.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to populate with the created vault reference.</param>
    [RequestHandler]
    public async Task CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        var vaultRef = await vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
    }

    /// <summary>
    ///     Closes the currently active vault.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to confirm closure.</param>
    [RequestHandler]
    public void CloseVault(ZyRequest<VaultCloseReq> req, ZyResponse<VaultCloseRes> res)
    {
        vaultService.CloseVault();
        res.SetData(new VaultCloseRes());
    }

    /// <summary>
    ///     Returns the current vault status and reference if any.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to populate with vault status.</param>
    [RequestHandler]
    public void GetStatus(ZyRequest<VaultGetStatusReq> req, ZyResponse<VaultGetStatusRes> res)
    {
        res.SetData(new VaultGetStatusRes { VaultRef = vaultService.GetActiveVaultRef() });
    }
}
