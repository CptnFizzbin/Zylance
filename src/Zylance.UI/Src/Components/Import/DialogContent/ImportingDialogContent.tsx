import {
  Button,
  DialogActions,
  DialogContent,
  LinearProgress,
} from "@mui/material"
import type { FC } from "react"

export interface ImportingDialogContentProps {
  onCancel: () => void
}

export const ImportingDialogContent: FC<ImportingDialogContentProps> = ({
  onCancel,
}) => (
  <>
    <DialogContent>
      Importing transactions...
      <LinearProgress />
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
    </DialogActions>
  </>
)
