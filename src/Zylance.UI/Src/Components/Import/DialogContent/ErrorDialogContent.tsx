import { Button, DialogActions, DialogContent } from "@mui/material"
import type { FC } from "react"

export interface ErrorDialogContentProps {
  onTryAgain: () => void
  onClose: () => void
}

export const ErrorDialogContent: FC<ErrorDialogContentProps> = ({
  onTryAgain,
  onClose,
}) => (
  <>
    <DialogContent>Error during import.</DialogContent>
    <DialogActions>
      <Button onClick={onTryAgain}>Try Again</Button>
      <Button onClick={onClose}>Close</Button>
    </DialogActions>
  </>
)
