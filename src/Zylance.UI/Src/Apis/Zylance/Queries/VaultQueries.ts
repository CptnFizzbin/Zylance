import { createQueryKeys } from "@lukemorales/query-key-factory"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"
import { ZylanceActions } from "$Generated/ZylanceConstants"

export const vaultQueries = (zylanceApi: ZylanceApi) => {
  return createQueryKeys("vault", {
    status: {
      queryKey: [ZylanceActions.Vault_GetStatus],
      queryFn: async () => {
        if (!zylanceApi) return null
        const status = await zylanceApi.vault.getStatus()
        return status.vaultRef ?? null
      },
    },
  })
}
