import { mergeQueryKeys } from "@lukemorales/query-key-factory"
import { useMemo } from "react"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { ledgerQueries } from "@/Apis/Zylance/Queries/LedgerQueries"
import { settingsQueries } from "@/Apis/Zylance/Queries/SettingsQueries"
import { vaultQueries } from "@/Apis/Zylance/Queries/VaultQueries"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"

export const zylanceQueries = (zylanceApi: ZylanceApi) => {
  return mergeQueryKeys(
    settingsQueries(zylanceApi),
    vaultQueries(zylanceApi),
    ledgerQueries(zylanceApi),
  )
}

export const useZylanceQueries = () => {
  const zylanceApi = useZylanceApi()
  return useMemo(() => zylanceQueries(zylanceApi), [zylanceApi])
}
