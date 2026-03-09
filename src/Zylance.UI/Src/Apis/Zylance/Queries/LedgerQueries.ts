import { createQueryKeys } from "@lukemorales/query-key-factory"
import type { ZylanceApi } from "@/Apis/Zylance/ZylanceApi"
import type { LedgerFilter } from "$Contract/api/Ledger"

export const ledgerQueries = (zylanceApi: ZylanceApi) => {
  return createQueryKeys("ledger", {
    list: (filter?: LedgerFilter) => ({
      queryKey: [filter],
      queryFn: async () => {
        const res = await zylanceApi.ledger.listLedgerEntries({ filter })
        return res.entries
      },
    }),
    entry: (id: string) => ({
      queryKey: [id],
      queryFn: () => zylanceApi.ledger.getLedgerEntry({ id }),
    }),
    search: (query: string, filter?: LedgerFilter) => ({
      queryKey: [query, filter],
      queryFn: () => zylanceApi.ledger.searchLedgerEntries({ query, filter }),
    }),
  })
}
