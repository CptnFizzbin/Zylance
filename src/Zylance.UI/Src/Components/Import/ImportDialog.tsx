import { Dialog, DialogTitle } from "@mui/material"
import type { FC } from "react"
import { CancelledDialogContent } from "@/Components/Import/DialogContent/CancelledDialogContent"
import { ErrorDialogContent } from "@/Components/Import/DialogContent/ErrorDialogContent"
import { FinishedDialogContent } from "@/Components/Import/DialogContent/FinishedDialogContent"
import { ImportingDialogContent } from "@/Components/Import/DialogContent/ImportingDialogContent"
import { ReadingDialogContent } from "@/Components/Import/DialogContent/ReadingDialogContent"
import { SelectFileDialogContent } from "@/Components/Import/DialogContent/SelectFileDialogContent"
import { SetAccountsDialogContent } from "@/Components/Import/DialogContent/SetAccountsDialogContent"
import { useImportService } from "@/Components/Import/ImportContext"

export interface ImportDialogProps {
  open: boolean
}

export const ImportDialog: FC<ImportDialogProps> = ({ open }) => {
  const { importStep, reset } = useImportService()

  let dialogContent = null
  switch (importStep) {
    case "selectFile":
      dialogContent = <SelectFileDialogContent />
      break
    case "reading":
      dialogContent = <ReadingDialogContent />
      break
    case "accounts":
      dialogContent = <SetAccountsDialogContent />
      break
    case "importing":
      dialogContent = <ImportingDialogContent />
      break
    case "finished":
      dialogContent = <FinishedDialogContent />
      break
    case "error":
      dialogContent = <ErrorDialogContent />
      break
    case "cancelled":
      dialogContent = <CancelledDialogContent />
      break
  }

  return (
    <Dialog open={open} slotProps={{ transition: { onExited: () => reset() } }}>
      <DialogTitle>Import Transactions</DialogTitle>
      <form>{dialogContent}</form>
    </Dialog>
  )
}
