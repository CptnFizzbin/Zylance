import {
  createContext,
  type FC,
  type PropsWithChildren,
  useCallback,
  useContext,
  useState,
} from "react"
import { ImportDialog } from "@/Components/Import/ImportDialog"

export interface ImportState {
  startImport: () => void
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

  const startImport = useCallback(() => {
    setDialogOpen(() => true)
  }, [])

  return (
    <ImportContext.Provider value={{ startImport }}>
      {children}
      <ImportDialog open={dialogOpen} onClose={() => setDialogOpen(false)} />
    </ImportContext.Provider>
  )
}
