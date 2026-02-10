import {
  createAccountApi,
  createBackgroundApi,
  createDesktopApi,
  createEchoApi,
  createFileApi,
  createLedgerApi,
  createStatusApi,
  createVaultApi,
} from "@Lib/ZylanceApi"
import { ZylanceClient } from "../ZylanceClient"

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
