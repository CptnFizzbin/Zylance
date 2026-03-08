import { mergeQueryKeys } from "@lukemorales/query-key-factory"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { settingsQueries } from "@/Apis/Zylance/Queries/SettingsQueries"
import { vaultQueries } from "@/Apis/Zylance/Queries/VaultQueries"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"

export const zylanceQueries = (zylanceApi: ZylanceApi) => {
  return mergeQueryKeys(settingsQueries(zylanceApi), vaultQueries(zylanceApi))
}

export const useZylanceQueries = () => {
  return zylanceQueries(useZylanceApi())
}
