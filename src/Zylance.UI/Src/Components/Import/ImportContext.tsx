import { useMutation } from "@tanstack/react-query"
import { createContext, type FC, type PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { catchError, EMPTY, filter } from "rxjs"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { ImportDialog } from "@/Components/Import/ImportDialog"
import { type ImportForm, useImportForm } from "@/Components/Import/UseImportForm"
import type {
  ImportCancelledEvt,
  ImportErrorEvt,
  ImportFinishedEvt,
  ImportReadingFileEvt,
  ImportSetAccountsEvt,
  ImportStartedEvt,
} from "$Contract/api/Import"
import type { AccountData } from "$Contract/models/Account"
import type { FileRef } from "$Contract/models/File"
import { ZylanceEvents } from "$Generated/ZylanceConstants"

export type ImportStep =
  | "selectFile"
  | "reading"
  | "accounts"
  | "importing"
  | "finished"
  | "error"
  | "cancelled"

export interface ImportState {
  openDialog: () => void
  closeDialog: () => void
  uploadFile: (fileRef: FileRef) => void
  cancelImport: () => void
  setAccounts: (accounts: AccountData[]) => void

  reset: () => void

  form: ImportForm

  importId: string | null
  importStep: ImportStep
  setImportStep: (importStep: ImportStep) => void

  error: Error | null
  setError: (error: Error | null) => void
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
  const zylanceApi = useZylanceApi()

  const form = useImportForm()

  const [dialogOpen, setDialogOpen] = useState(false)
  const [importId, setImportId] = useState<string | null>(null)
  const [importStep, setImportStep] = useState<ImportStep>("selectFile")
  const [importError, setImportError] = useState<Error | null>(null)

  const onError = useCallback((error: unknown) => {
    console.error("Import error:", error)
    setImportStep("error")
    setImportError(error instanceof Error ? error : new Error(String(error)))
  }, [])

  const onReset = () => {
    form.reset()
    setImportId(null)
    setImportStep("selectFile")
  }

  const uploadFile = useMutation({
    mutationFn: async (fileRef: FileRef) => {
      if (!fileRef) return
      const { importId } = await zylanceApi.import.uploadFile({ fileRef })
      setImportId(importId)
      setImportStep("reading")
    },
    onError: onError,
  })

  const cancelImport = useMutation({
    mutationFn: async () => {
      if (!importId) return
      zylanceApi.import.cancelImport({ importId })
      setImportStep("cancelled")
    },
    onError: onError,
  })

  const setAccounts = useMutation({
    mutationFn: async (accounts: AccountData[]) => {
      if (!importId) return
      zylanceApi.import.setAccounts({ importId, accounts: accounts })
    },
    onError: onError,
  })

  useEffect(() => {
    if (!importId) return

    function observeImportEvents<TEvt extends { importId: string }> (
      eventName: string,
    ) {
      return zylanceApi
        .observeEvent<TEvt>(eventName)
        .pipe(filter((evt) => evt.importId === importId))
        .pipe(
          catchError((error: unknown) => {
            onError(error)
            return EMPTY
          }),
        )
    }

    const subscriptions = [
      observeImportEvents<ImportReadingFileEvt>(
        ZylanceEvents.Import_ReadingFile,
      ).subscribe(() => setImportStep("reading")),
      observeImportEvents<ImportSetAccountsEvt>(
        ZylanceEvents.Import_GetAccounts,
      ).subscribe((evt) => {
        form.setFieldValue("accounts", evt.accounts ?? [])
        setImportStep("accounts")
      }),
      observeImportEvents<ImportStartedEvt>(
        ZylanceEvents.Import_Started,
      ).subscribe(() => setImportStep("importing")),
      observeImportEvents<ImportFinishedEvt>(
        ZylanceEvents.Import_Finished,
      ).subscribe(() => setImportStep("finished")),
      observeImportEvents<ImportCancelledEvt>(
        ZylanceEvents.Import_Cancelled,
      ).subscribe(() => setImportStep("cancelled")),
      observeImportEvents<ImportErrorEvt>(ZylanceEvents.Import_Error).subscribe(
        ({ errorMessage }) => {
          setImportStep("error")
          setImportError(new Error(errorMessage))
        },
      ),
    ]

    return () => subscriptions.forEach((sub) => void sub.unsubscribe())
  }, [form, zylanceApi, importId, onError])

  const state: ImportState = {
    openDialog: () => setDialogOpen(true),
    closeDialog: () => setDialogOpen(false),
    reset: onReset,

    form,

    importId,
    importStep,
    setImportStep,

    error: importError,
    setError: onError,

    cancelImport: () => cancelImport.mutate(),
    uploadFile: (fileRef) => uploadFile.mutate(fileRef),
    setAccounts: (accounts) => setAccounts.mutate(accounts),
  }

  return (
    <ImportContext.Provider value={state}>
      {children}
      <ImportDialog open={dialogOpen} />
    </ImportContext.Provider>
  )
}
