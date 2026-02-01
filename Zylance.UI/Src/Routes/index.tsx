import type { FileRef } from "@Contract/models/File"
import type { VaultRef } from "@Contract/models/Vault"
import { useZylance } from "@Lib/ZylanceContext"
import { createFileRoute } from "@tanstack/react-router"
import { useState } from "react"
import logo from "../logo.svg"

export const Route = createFileRoute("/")({
  component: App,
})

function App () {
  const zylanceApi = useZylance()
  const [lastMessage, setLastMessage] = useState("")
  const [error, setError] = useState<string | null>(null)
  const [selectedFile, setSelectedFile] = useState<FileRef | null>(null)
  const [openedVault, setOpenedVault] = useState<VaultRef | null>(null)

  const onBtnClick = async () => {
    const res = await zylanceApi.echo.echoMessage({ message: "Hello from Zylence!" })
    setLastMessage(res.echoed)
  }

  const onSelectFileClick = async () => {
    try {
      const { fileRef } = await zylanceApi.files.selectFile({
        title: "Select a text file",
        filters: [
          { name: "Text Files", extensions: ["txt", "md"] },
          { name: "All Files", extensions: ["*"] },
        ],
        readOnly: true,
      })

      if (fileRef) {
        setError(null)
        setSelectedFile(fileRef)
      } else {
        setError("No file selected")
        setSelectedFile(null)
      }
      setSelectedFile(fileRef ?? null)
    } catch (error) {
      console.error("File selection error:", error)
      setSelectedFile(null)
      setError("Error selecting file")
    }
  }

  const onOpenVaultClick = async () => {
    try {
      const { vaultRef } = await zylanceApi.vault.openVault()
      setOpenedVault(vaultRef ?? null)
    } catch (error) {
      console.error("Vault open error:", error)
      setError("Error opening vault")
    }
  }

  return (
    <div className="text-center">
      <header className="min-h-screen flex flex-col items-center justify-center bg-[#282c34] text-white text-[calc(10px+2vmin)]">
        <img
          src={logo}
          className="h-[40vmin] pointer-events-none animate-[spin_20s_linear_infinite]"
          alt="logo"
        />
        <p>
          Edit <code>src/routes/index.tsx</code> and save to reload
        </p>

        <div>
          {error && <p className="text-red-500">Error: {error}</p>}
        </div>

        <div className="space-y-4 mt-4">
          <button type="button" onClick={onBtnClick} className="mx-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded">
            Send Message to Backend
          </button>
          {lastMessage && <p>Last message from backend: {lastMessage}</p>}

          <button
            type="button"
            onClick={onSelectFileClick}
            className="mx-2 px-4 py-2 bg-green-600 hover:bg-green-700 rounded"
          >
            Select File
          </button>
          {selectedFile && (
            <div className="mt-2">
              <p>Selected file: {selectedFile.filename}</p>
            </div>
          )}

          <button
            type="button"
            onClick={onOpenVaultClick}
            className="mx-2 px-4 py-2 bg-purple-600 hover:bg-purple-700 rounded"
          >
            Open Vault
          </button>
          {openedVault && (
            <div className="mt-2">
              <p>Opened vault: {openedVault.id}</p>
            </div>
          )}
        </div>

        <a
          className="text-[#61dafb] hover:underline"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
        <a
          className="text-[#61dafb] hover:underline"
          href="https://tanstack.com"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn TanStack
        </a>
      </header>
    </div>
  )
}
