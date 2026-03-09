import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"
import { useUpdateLedgerEntry } from "./LedgerEntryMutations"
import type { LedgerEntryData } from "$Contract/models/Ledger"

export interface UseLedgerRowFormProps {
  ledgerEntry: LedgerEntryData
}

export const useLedgerRowForm = ({ ledgerEntry }: UseLedgerRowFormProps) => {
  const updateMutation = useUpdateLedgerEntry()

  return useAppForm({
    defaultValues: ledgerEntry,
    onSubmit: async ({ value }) => {
      try {
        await updateMutation.mutateAsync({ id: value.id, entry: value })
      } catch (err) {
        console.error("Failed to save ledger entry:", err)
        throw err
      }
    },
  })
}
