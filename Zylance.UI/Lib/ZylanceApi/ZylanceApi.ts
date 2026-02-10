import { ZylanceClient } from "../ZylanceClient"
import { createAccountApi } from "./AccountApi"
import { createBackgroundApi } from "./BackgroundApi"
import { createDesktopApi } from "./DesktopApi"
import { createEchoApi } from "./EchoApi"
import { createFileApi } from "./FileApi"
import { createLedgerApi } from "./LedgerApi"
import { createStatusApi } from "./StatusApi"
import { createVaultApi } from "./VaultApi"

export function createZylanceApi() {
  const client = new ZylanceClient()

  return {
    observeEvent: client.observeEvent.bind(client),
    desktop: createDesktopApi(client),
    status: createStatusApi(client),
    echo: createEchoApi(client),
    files: createFileApi(client),
    vault: createVaultApi(client),
    background: createBackgroundApi(client),
    account: createAccountApi(client),
    ledger: createLedgerApi(client),
  }
}

export type ZylanceApi = ReturnType<typeof createZylanceApi>
