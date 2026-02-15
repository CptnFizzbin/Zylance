import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const ErrorDialogContent: FC = () => {
  const { reset, closeDialog } = useImportService()

  return (
    <>
      <DialogContent>Error during import.</DialogContent>
      <DialogActions>
        <Button onClick={reset}>Try Again</Button>
        <Button onClick={closeDialog}>Close</Button>
      </DialogActions>
    </>
  )
}
