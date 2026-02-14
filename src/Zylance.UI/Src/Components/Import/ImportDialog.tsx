import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from "@mui/material"
import type { FC } from "react"
import { SelectFileStage } from "@/Components/Import/ImportStage/SelectFileStage"

export interface ImportDialogProps {
  open: boolean
  onClose?: () => void
  onCancel?: () => void
}

export const ImportDialog: FC<ImportDialogProps> = ({
  open,
  onClose,
  onCancel,
}) => {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Import Transactions</DialogTitle>
      <DialogContent>
        <SelectFileStage />
      </DialogContent>
      <DialogActions>
        <Stack direction="row" justifyContent={"space-between"} flexGrow={1}>
          <Button onClick={onCancel}>Cancel</Button>
          <Button onClick={onClose} variant="contained">
            Close
          </Button>
        </Stack>
      </DialogActions>
    </Dialog>
  )
}
