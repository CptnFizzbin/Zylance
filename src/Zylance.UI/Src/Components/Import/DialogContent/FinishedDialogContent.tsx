import { Button, DialogActions, DialogContent } from "@mui/material"
import { useQueryClient } from "@tanstack/react-query"
import { type FC, useEffect } from "react"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"
import { useImportService } from "@/Components/Import/ImportContext"

export const FinishedDialogContent: FC = () => {
  const { closeDialog } = useImportService()
  const zylanceQueries = useZylanceQueries()
  const queryClient = useQueryClient()

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: zylanceQueries.ledger._def })
  }, [zylanceQueries, queryClient])

  return (
    <>
      <DialogContent>Import finished!</DialogContent>
      <DialogActions>
        <Button onClick={closeDialog}>Close</Button>
      </DialogActions>
    </>
  )
}
