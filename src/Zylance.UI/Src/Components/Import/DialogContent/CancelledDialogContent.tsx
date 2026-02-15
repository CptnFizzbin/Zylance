import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const CancelledDialogContent: FC = () => {
  const { closeDialog } = useImportService()

  return (
    <>
      <DialogContent>Import cancelled.</DialogContent>
      <DialogActions>
        <Button onClick={closeDialog}>Close</Button>
      </DialogActions>
    </>
  )
}
