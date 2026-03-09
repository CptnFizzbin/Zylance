using Serilog;
using Zylance.Contract.Api.Settings;
using Zylance.Core.Gateway.Models;
using Zylance.Core.Logging;
using Zylance.Core.Router.Attributes;
using Zylance.Core.Settings.Models;
using Zylance.Core.Settings.Services;

namespace Zylance.Core.Router.Controllers;

/// <summary>
///     Controller for application settings endpoints.
/// </summary>
[Controller]
public class SettingsController(UserPreferencesService settingsService)
{
    private static readonly ILogger Log = ZyLogger.ForContext<SettingsController>();

    /// <summary>
    ///     Returns available and current date/time formats and timezone.
    /// </summary>
    [RequestHandler]
    public async Task GetUserPreferences(ZyRequest<GetUserPreferencesReq> _, ZyResponse<GetUserPreferencesRes> res)
    {
        Log.Debug("GetUserPreferences called");
        var settings = await settingsService.LoadUserPreferencesAsync();

        res.SetData(new() { Preferences = settings.ToData() });
    }

    /// <summary>
    ///     Updates date/time preferences and persists them.
    /// </summary>
    [RequestHandler]
    public async Task SetUserPreferences(ZyRequest<SetUserPreferencesReq> req, ZyResponse<SetUserPreferencesRes> res)
    {
        var data = req.GetData();
        Log.Debug("SetUserPreferences called Preferences={Preferences}", data.Preferences);

        var settings = UserPreferencesSettings.FromData(data.Preferences);
        var savedSettings = await settingsService.SaveUserPreferencesAsync(settings);

        res.SetData(new() { Preferences = savedSettings.ToData() });
    }

    /// <summary>
    ///     Returns available date/time options (formats, timezones, etc.).
    /// </summary>
    [RequestHandler]
    public Task GetDateTimeOptions(ZyRequest<GetDateTimeOptionsReq> _, ZyResponse<GetDateTimeOptionsRes> res)
    {
        Log.Debug("GetDateTimeOptions called");
        res.SetData(new() { Options = DateTimeOptions.ToData() });
        return Task.CompletedTask;
    }
}
