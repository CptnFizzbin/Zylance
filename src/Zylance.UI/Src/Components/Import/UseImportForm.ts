import type { FileRef } from "@Contract/models/File"
import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"

interface FormState {
  importFile: FileRef | null
}

const defaultValues: FormState = {
  importFile: null,
}

export const useImportForm = () => {
  return useAppForm({
    defaultValues,
  })
}

export type ImportForm = ReturnType<typeof useImportForm>
