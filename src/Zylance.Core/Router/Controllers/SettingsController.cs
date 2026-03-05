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
public class SettingsController
{
    private static readonly ILogger Log = ZyLogger.ForContext<SettingsController>();
    private readonly UserPreferencesService _settingsService;

    public SettingsController(UserPreferencesService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    ///     Returns available and current date/time formats and timezone.
    /// </summary>
    [RequestHandler]
    public async Task GetDateTimeSettings(ZyRequest<GetDateTimeSettingsReq> req, ZyResponse<GetDateTimeSettingsRes> res)
    {
        Log.Debug("GetDateTimeSettings called");
        var settings = await _settingsService.LoadUserPreferencesAsync();

        var available = new DateTimeAvailableFormats();
        available.Date.AddRange(DateAndTimeSettings.DateFormats);
        available.Time.AddRange(DateAndTimeSettings.TimeFormats);

        var current = new DateTimeCurrentFormats
        {
            Date = settings.DateTime.DateFormat,
            Time = settings.DateTime.TimeFormat,
        };

        var timezone = settings.DateTime.TimeZone.Id; // return system tz id; UI may prefer 'system' or offset

        res.SetData(
            new GetDateTimeSettingsRes
            {
                Available = available,
                Current = current,
                Timezone = timezone,
            }
        );
        Log.Debug("GetDateTimeSettings responded");
    }

    /// <summary>
    ///     Updates date/time preferences and persists them.
    /// </summary>
    [RequestHandler]
    public async Task UpdateDateTimeSettings(
        ZyRequest<UpdateDateTimeSettingsReq> req,
        ZyResponse<UpdateDateTimeSettingsRes> res
    )
    {
        var data = req.GetData();
        Log.Debug(
            "UpdateDateTimeSettings called Date={Date} Time={Time} Timezone={Timezone}",
            data.Date,
            data.Time,
            data.Timezone
        );

        var settings = await _settingsService.LoadUserPreferencesAsync();

        var dt = settings.DateTime with { };

        if (data.Date is not null)
            dt = dt with { DateFormat = data.Date };

        if (data.Time is not null)
            dt = dt with { TimeFormat = data.Time };

        if (data.Timezone is not null)
            try
            {
                // Accept 'system' to mean local system timezone, otherwise try to find by id or parse as offset
                if (data.Timezone == "system")
                {
                    dt = dt with { TimeZone = TimeZoneInfo.Local };
                }
                else if (TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id == data.Timezone))
                {
                    dt = dt with { TimeZone = TimeZoneInfo.FindSystemTimeZoneById(data.Timezone) };
                }
                else
                {
                    TimeSpan offset;
                    if (TimeSpan.TryParse(data.Timezone, out offset))
                    {
                        // create custom timezone from offset
                        var tz = TimeZoneInfo.CreateCustomTimeZone(
                            $"UTC{offset}",
                            offset,
                            $"UTC{offset}",
                            $"UTC{offset}"
                        );
                        dt = dt with { TimeZone = tz };
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to parse timezone {Timezone}", data.Timezone);
            }

        var newSettings = settings with { DateTime = dt };
        await _settingsService.SaveUserPreferencesAsync(newSettings);

        var response = new UpdateDateTimeSettingsRes
        {
            Current = new DateTimeCurrentFormats
            {
                Date = newSettings.DateTime.DateFormat,
                Time = newSettings.DateTime.TimeFormat,
            },
            Timezone = newSettings.DateTime.TimeZone.Id,
        };

        res.SetData(response);
        Log.Debug("UpdateDateTimeSettings responded");
    }
}
