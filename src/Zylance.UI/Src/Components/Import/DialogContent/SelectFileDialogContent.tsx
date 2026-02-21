import type { FileFilter } from "$Contract/models/File"
import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const SelectFileDialogContent: FC = () => {
  const { form, cancelImport, uploadFile } = useImportService()

  // TODO: This should come from the backend, but for now we can hardcode it here
  const fileFilters: FileFilter[] = [
    { name: "Quicken", extensions: ["qfx"] },
    { name: "OpenFinance", extensions: ["ofx"] },
  ]

  const allSupportedFilesFilter: FileFilter = {
    name: "Supported Files",
    extensions: fileFilters.flatMap((filter) => filter.extensions),
  }

  return (
    <>
      <DialogContent>
        <form.AppField name={"importFile"}>
          {(field) => (
            <field.FilePickerField
              label="Select File"
              title="Select a file to import"
              mode={"select"}
              filters={[allSupportedFilesFilter, ...fileFilters]}
              readonly
            />
          )}
        </form.AppField>
      </DialogContent>
      <form.Subscribe selector={(state) => state.values.importFile}>
        {(fileRef) => (
          <DialogActions>
            <Button onClick={cancelImport}>Cancel</Button>
            <Button
              disabled={!fileRef}
              onClick={() => {
                if (!fileRef) return
                uploadFile(fileRef)
              }}
            >
              Next
            </Button>
          </DialogActions>
        )}
      </form.Subscribe>
    </>
  )
}
