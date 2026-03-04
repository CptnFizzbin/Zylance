using Serilog;
using Zylance.Contract.Api.Vault;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;
using Zylance.Core.Vault.Services;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller exposing vault lifecycle operations (open/create/close/status).
/// </summary>
[Controller]
public class VaultController(VaultService vaultService, RecentVaultsService recentVaultsService)
{
    private static readonly ILogger Log = ZyLogger.ForContext<VaultController>();

    /// <summary>
    ///     Opens the active vault and returns its reference.
    /// </summary>
    /// <param name="req">Request object</param>
    /// <param name="res">Response to populate with the opened vault reference.</param>
    [RequestHandler]
    public async Task OpenVault(ZyRequest<VaultOpenReq> req, ZyResponse<VaultOpenRes> res)
    {
        Log.Debug("OpenVault called");
        var vaultRef = await vaultService.OpenVault();
        res.SetData(new VaultOpenRes { VaultRef = vaultRef });
        Log.Information("Opened vault {VaultRef}", vaultRef);
    }

    /// <summary>
    ///     Creates a new vault and sets it as active, returning the vault reference.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to populate with the created vault reference.</param>
    [RequestHandler]
    public async Task CreateVault(ZyRequest<VaultCreateReq> req, ZyResponse<VaultCreateRes> res)
    {
        Log.Debug("CreateVault called");
        var vaultRef = await vaultService.CreateVault();
        res.SetData(new VaultCreateRes { VaultRef = vaultRef });
        Log.Information("Created vault {VaultRef}", vaultRef);
    }

    /// <summary>
    ///     Closes the currently active vault.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to confirm closure.</param>
    [RequestHandler]
    public void CloseVault(ZyRequest<VaultCloseReq> req, ZyResponse<VaultCloseRes> res)
    {
        Log.Debug("CloseVault called");
        vaultService.CloseVault();
        res.SetData(new VaultCloseRes());
        Log.Information("Closed active vault");
    }

    /// <summary>
    ///     Returns the current vault status and reference if any.
    /// </summary>
    /// <param name="req">Request object (unused).</param>
    /// <param name="res">Response to populate with vault status.</param>
    [RequestHandler]
    public void GetStatus(ZyRequest<VaultGetStatusReq> req, ZyResponse<VaultGetStatusRes> res)
    {
        Log.Debug("Vault GetStatus called");
        res.SetData(new VaultGetStatusRes { VaultRef = vaultService.GetActiveVaultRef() });
        Log.Debug("Vault GetStatus responded");
    }

    /// <summary>
    ///     Returns recent vaults for a provider (defaults to "desktop").
    /// </summary>
    [RequestHandler]
    public async Task ListRecentVaults(ZyRequest<ListRecentVaultsReq> req, ZyResponse<ListRecentVaultsRes> res)
    {
        var provider = string.IsNullOrWhiteSpace(req.Data.Provider) ? "desktop" : req.Data.Provider;
        Log.Debug("ListRecentVaults called for provider {Provider}", provider);

        var recent = await recentVaultsService.GetRecentVaultsAsync(provider);

        var resp = new ListRecentVaultsRes();
        resp.RecentVaults.AddRange(recent);
        res.SetData(resp);

        Log.Debug("ListRecentVaults responded with {Count} entries", resp.RecentVaults.Count);
    }
}
