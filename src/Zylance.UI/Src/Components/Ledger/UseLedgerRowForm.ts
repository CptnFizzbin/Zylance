import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"
import type { LedgerEntryData } from "$Contract/models/Ledger"

export interface UseLedgerRowFormProps {
  ledgerEntry: LedgerEntryData
}

export const useLedgerRowForm = ({ ledgerEntry }: UseLedgerRowFormProps) => {
  return useAppForm({
    defaultValues: ledgerEntry,
    onSubmit: async (values) => {
      console.log("Submitting form with values:", values)
      // TODO: Save the updated ledger entry using the API
    },
  })
}

export type LedgerRowForm = ReturnType<typeof useLedgerRowForm>
