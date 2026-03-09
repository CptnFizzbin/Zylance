import { createQueryKeys } from "@lukemorales/query-key-factory"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"

export const accountsQueries = (zylanceApi: ZylanceApi) => {
  return createQueryKeys("accounts", {
    list: {
      queryKey: null,
      queryFn: async () => {
        const res = await zylanceApi.account.listAccounts({})
        return res.accounts
      },
    },
    get: (accountId: string) => ({
      queryKey: [accountId],
      queryFn: async () => {
        const res = await zylanceApi.account.getAccount({ accountId })
        return res.account
      },
    }),
  })
}
