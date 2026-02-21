import type { LedgerEntryData } from "$Contract/models/Ledger"
import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"

export interface LedgerEntryRowData extends LedgerEntryData {
  credit: string
  debit: string
}

export interface UseLedgerRowFormProps {
  ledgerEntry: LedgerEntryRowData
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
