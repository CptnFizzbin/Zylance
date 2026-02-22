import { createQueryKeys } from "@lukemorales/query-key-factory"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import type * as LedgerTypes from "$Contract/api/Ledger"
import type {
  CreateLedgerEntryReq,
  DeleteLedgerEntryReq,
  UpdateLedgerEntryReq,
} from "$Contract/api/Ledger"
import type { VaultRef } from "$Contract/models/Vault"

// Query Key Factory for ledger queries
export const ledgerQueryKeys = createQueryKeys("ledger-entries", {
  inVault: (vaultRef: VaultRef | null) => ({
    queryKey: [`vault/${vaultRef?.id}`],
    contextQueries: {
      list: (filter?: LedgerTypes.LedgerFilter) => [filter],
      entry: (id: string) => [id],
      search: (query: string, filter?: LedgerTypes.LedgerFilter) => [
        query,
        filter,
      ],
    },
  }),
})

// Fetch all ledger entries (with optional filter/pagination)
export function useLedgerEntries(
  vaultRef: VaultRef | null,
  filter?: LedgerTypes.LedgerFilter,
) {
  const zylanceApi = useZylanceApi()

  return useQuery({
    ...ledgerQueryKeys.inVault(vaultRef)._ctx.list(filter),
    enabled: !!vaultRef,
    queryFn: async () => {
      if (!vaultRef) return []
      const res = await zylanceApi.ledger.listLedgerEntries({})
      return res.entries
    },
    staleTime: "static",
  })
}

// Fetch a single ledger entry by ID
export function useLedgerEntry(vaultRef: VaultRef, id: string) {
  const zylanceApi = useZylanceApi()

  return useQuery({
    ...ledgerQueryKeys.inVault(vaultRef)._ctx.entry(id),
    queryFn: () => zylanceApi.ledger.getLedgerEntry({ id }),
    enabled: !!id,
  })
}

// Search ledger entries (vaultRef not required)
export function useSearchLedgerEntries(
  vaultRef: VaultRef | null,
  query: string,
  filter?: LedgerTypes.LedgerFilter,
) {
  const zylanceApi = useZylanceApi()

  return useQuery({
    ...ledgerQueryKeys.inVault(vaultRef)._ctx.search(query, filter),
    queryFn: () => zylanceApi.ledger.searchLedgerEntries({ query, filter }),
    enabled: !!query,
  })
}

// Create a ledger entry
export function useCreateLedgerEntry() {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: CreateLedgerEntryReq) =>
      zylanceApi.ledger.createLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ledgerQueryKeys._def })
    },
  })
}

// Update a ledger entry (vaultRef not required)
export function useUpdateLedgerEntry() {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: UpdateLedgerEntryReq) =>
      zylanceApi.ledger.updateLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ledgerQueryKeys._def })
    },
  })
}

// Delete a ledger entry (vaultRef not required)
export function useDeleteLedgerEntry() {
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: DeleteLedgerEntryReq) =>
      zylanceApi.ledger.deleteLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ledgerQueryKeys._def })
    },
  })
}
