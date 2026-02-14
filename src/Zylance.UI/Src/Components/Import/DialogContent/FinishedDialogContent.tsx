import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"

export interface FinishedDialogContentProps {
  onClose: () => void
}

export const FinishedDialogContent: FC<FinishedDialogContentProps> = ({
  onClose,
}) => (
  <>
    <DialogContent>Import finished!</DialogContent>
    <DialogActions>
      <Button onClick={onClose}>Close</Button>
    </DialogActions>
  </>
)
