import type { FileRef, FileFilter } from "@Contract/models/File"
import type { FC } from "react"
import { useAppForm } from "@/Integrations/tanstack-form/UseAppForm"

interface FormState {
  fileRef: FileRef | null
}

const defaultValues: FormState = {
  fileRef: null,
}

export const SelectFileStage: FC = () => {
  const form = useAppForm({
    defaultValues,
  })

  const fileFilters: FileFilter[] = [
    { name: "Quicken Finance", extensions: ["*.qfx"] },
    { name: "Open Finance", extensions: ["*.ofx"] },
  ]

  const supportedFilesFilter: FileFilter = {
    name: "Supported Files",
    extensions: fileFilters.flatMap(e => e.extensions),
  }

  return (
    <form>
      <form.AppField
        name="fileRef"
        children={(field) => <field.FilePickerField
          label={"Select File"} mode={"select"} filters={[
          supportedFilesFilter,
          ...fileFilters,
        ]}
        />}
      />
    </form>
  )
}
