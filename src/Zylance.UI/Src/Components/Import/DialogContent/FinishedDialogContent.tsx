import { Button, DialogActions, DialogContent } from "@mui/material"
import { useQueryClient } from "@tanstack/react-query"
import { type FC, useEffect } from "react"
import { useImportService } from "@/Components/Import/ImportContext"
import { ledgerQueryKeys } from "@/Components/Ledger/LedgerEntryQueries"

export const FinishedDialogContent: FC = () => {
  const { closeDialog } = useImportService()
  const queryClient = useQueryClient()

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ledgerQueryKeys._def })
  }, [queryClient])

  return (
    <>
      <DialogContent>Import finished!</DialogContent>
      <DialogActions>
        <Button onClick={closeDialog}>Close</Button>
      </DialogActions>
    </>
  )
}
