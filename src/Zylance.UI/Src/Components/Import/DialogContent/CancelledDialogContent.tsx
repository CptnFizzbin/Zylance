import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"

export interface CancelledDialogContentProps {
  onClose: () => void
}

export const CancelledDialogContent: FC<CancelledDialogContentProps> = ({
  onClose,
}) => (
  <>
    <DialogContent>Import cancelled.</DialogContent>
    <DialogActions>
      <Button onClick={onClose}>Close</Button>
    </DialogActions>
  </>
)
