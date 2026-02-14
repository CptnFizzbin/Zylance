import {
  Button,
  DialogActions,
  DialogContent,
  LinearProgress,
} from "@mui/material"
import type { FC } from "react"

export interface ReadingDialogContentProps {
  onCancel: () => void
}

export const ReadingDialogContent: FC<ReadingDialogContentProps> = ({
  onCancel,
}) => (
  <>
    <DialogContent>
      Reading file...
      <LinearProgress />
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
    </DialogActions>
  </>
)
