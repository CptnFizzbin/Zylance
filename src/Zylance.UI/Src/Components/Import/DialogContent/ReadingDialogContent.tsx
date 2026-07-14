import {
  Button,
  DialogActions,
  DialogContent,
  LinearProgress,
} from "@mui/material"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const ReadingDialogContent: FC = () => {
  const { cancelImport } = useImportService()

  return (
    <>
      <DialogContent>
        Reading file...
        <LinearProgress />
      </DialogContent>
      <DialogActions>
        <Button onClick={cancelImport}>Cancel</Button>
      </DialogActions>
    </>
  )
}
