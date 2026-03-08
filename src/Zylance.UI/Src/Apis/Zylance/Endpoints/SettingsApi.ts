import type * as SettingsTypes from "@Contract/api/Settings"
import type { ZylanceClient } from "@/Apis/Zylance/ZylanceClient"
import { ZylanceActions } from "$Generated/ZylanceConstants"

export const createSettingsApi = (client: ZylanceClient) => {
  return {
    getDateTimeOptions: client.createRequestEndpoint<
      typeof ZylanceActions.Settings_GetDateTimeOptions,
      void,
      SettingsTypes.GetDateTimeOptionsRes
    >(ZylanceActions.Settings_GetDateTimeOptions),

    userPreferences: {
      get: client.createRequestEndpoint<
        typeof ZylanceActions.Settings_GetUserPreferences,
        void,
        SettingsTypes.GetUserPreferencesRes
      >(ZylanceActions.Settings_GetUserPreferences),

      set: client.createRequestEndpoint<
        typeof ZylanceActions.Settings_GetUserPreferences,
        SettingsTypes.SetUserPreferencesReq,
        SettingsTypes.SetUserPreferencesRes
      >(ZylanceActions.Settings_GetUserPreferences),
    },
  }
}

export type SettingsApi = ReturnType<typeof createSettingsApi>
