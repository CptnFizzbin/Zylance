import { useMutation, useQueryClient } from "@tanstack/react-query"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"
import type { CreateLedgerEntryReq, DeleteLedgerEntryReq, UpdateLedgerEntryReq } from "$Contract/api/Ledger"

// Create a ledger entry
export function useCreateLedgerEntry () {
  const zylanceQueries = useZylanceQueries()
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: CreateLedgerEntryReq) =>
      zylanceApi.ledger.createLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: zylanceQueries.ledger._def })
    },
  })
}

// Update a ledger entry
export function useUpdateLedgerEntry () {
  const zylanceQueries = useZylanceQueries()
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: UpdateLedgerEntryReq) =>
      zylanceApi.ledger.updateLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: zylanceQueries.ledger._def })
    },
  })
}

// Delete a ledger entry
export function useDeleteLedgerEntry () {
  const zylanceQueries = useZylanceQueries()
  const zylanceApi = useZylanceApi()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (req: DeleteLedgerEntryReq) =>
      zylanceApi.ledger.deleteLedgerEntry(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: zylanceQueries.ledger._def })
    },
  })
}
