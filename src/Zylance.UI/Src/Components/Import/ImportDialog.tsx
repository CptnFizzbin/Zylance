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
  onClose?: () => void
  afterClose?: () => void
}

export const ImportDialog: FC<ImportDialogProps> = ({
  open,
  onClose,
  afterClose,
}) => {
  const { importStep, setImportStep } = useImportService()

  let dialogContent = null
  switch (importStep) {
    case "selectFile":
      dialogContent = <SelectFileDialogContent />
      break
    case "reading":
      dialogContent = (
        <ReadingDialogContent
          onCancel={onClose ?? (() => setImportStep("cancelled"))}
        />
      )
      break
    case "setAccounts":
      dialogContent = (
        <SetAccountsDialogContent
          onCancel={onClose ?? (() => setImportStep("cancelled"))}
          onImport={() => setImportStep("importing")}
        />
      )
      break
    case "importing":
      dialogContent = (
        <ImportingDialogContent
          onCancel={onClose ?? (() => setImportStep("cancelled"))}
        />
      )
      break
    case "finished":
      dialogContent = (
        <FinishedDialogContent
          onClose={onClose ?? (() => setImportStep("selectFile"))}
        />
      )
      break
    case "error":
      dialogContent = (
        <ErrorDialogContent
          onTryAgain={() => setImportStep("selectFile")}
          onClose={onClose ?? (() => setImportStep("selectFile"))}
        />
      )
      break
    case "cancelled":
      dialogContent = (
        <CancelledDialogContent
          onClose={onClose ?? (() => setImportStep("selectFile"))}
        />
      )
      break
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      slotProps={{ transition: { onExited: afterClose } }}
    >
      <DialogTitle>Import Transactions</DialogTitle>
      <form>{dialogContent}</form>
    </Dialog>
  )
}
