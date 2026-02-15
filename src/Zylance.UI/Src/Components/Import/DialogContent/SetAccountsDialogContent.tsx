import { Button, DialogActions, DialogContent, Typography } from "@mui/material"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const SetAccountsDialogContent: FC = () => {
  const { cancelImport, setAccounts } = useImportService()

  return (
    <>
      <DialogContent>
        <Typography>Placeholder!</Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={cancelImport}>Cancel</Button>
        <Button onClick={setAccounts}>Import</Button>
      </DialogActions>
    </>
  )
}
