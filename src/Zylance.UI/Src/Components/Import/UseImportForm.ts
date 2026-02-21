import type { AccountData } from "$Contract/models/Account"
import type { FileRef } from "$Contract/models/File"
import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"

interface FormState {
  importFile: FileRef | null
  accounts: AccountData[]
}

const defaultValues: FormState = {
  importFile: null,
  accounts: [],
}

export const useImportForm = () => {
  return useAppForm({
    defaultValues,
  })
}

export type ImportForm = ReturnType<typeof useImportForm>
