import { createContext, type FC, type PropsWithChildren, useContext, useState, useMemo } from "react"
import { ImportDialog } from "@/Components/Import/ImportDialog"
import type { FileRef } from "@Contract/models/File"

export interface ImportState {
  startImport: () => void
  reset: () => void

  importFile: FileRef | null

  setImportFile: (fileRef: FileRef) => void
}

export const ImportContext = createContext<ImportState | null>(null)

export const useImportService = () => {
  const context = useContext(ImportContext)
  if (!context) {
    throw new Error("useImportService must be used within an ImportProvider")
  }
  return context
}

export const ImportProvider: FC<PropsWithChildren> = ({ children }) => {
  const [dialogOpen, setDialogOpen] = useState(false)
  const [importFile, setImportFile] = useState<FileRef | null>(null)

  const startImport = () => setDialogOpen(true)

  const reset = () => {
    setImportFile(null)
  }

  const state = {
    startImport,
    reset,

    importFile,
    setImportFile,
  }

  const memoizedChildren = useMemo(() => children, [children])

  return (
    <ImportContext.Provider value={state}>
      {memoizedChildren}
      <ImportDialog open={dialogOpen} onClose={() => setDialogOpen(false)} />
    </ImportContext.Provider>
  )
}
