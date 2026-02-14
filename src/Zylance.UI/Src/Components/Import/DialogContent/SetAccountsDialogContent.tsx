import { Button, DialogActions, DialogContent, Typography } from "@mui/material"
import type { FC } from "react"

export interface SetAccountsDialogContentProps {
  onCancel: () => void
  onImport: () => void
}

export const SetAccountsDialogContent: FC<SetAccountsDialogContentProps> = ({
  onCancel,
  onImport,
}) => (
  <>
    <DialogContent>
      <Typography>Placeholder!</Typography>
    </DialogContent>
    <DialogActions>
      <Button onClick={onCancel}>Cancel</Button>
      <Button onClick={onImport}>Import</Button>
    </DialogActions>
  </>
)
