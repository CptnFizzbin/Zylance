import { createQueryKeys } from "@lukemorales/query-key-factory"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"
import { ZylanceActions } from "$Generated/ZylanceConstants"

export const settingsQueries = (zylanceApi: ZylanceApi) => {
  return createQueryKeys("settings", {
    dateTimeOptions: {
      queryKey: [ZylanceActions.Settings_GetDateTimeOptions],
      queryFn: () => zylanceApi.settings.getDateTimeOptions(),
    },
    userPreferences: {
      queryKey: [ZylanceActions.Settings_GetUserPreferences],
      queryFn: () => zylanceApi.settings.userPreferences.get(),
    },
  })
}
