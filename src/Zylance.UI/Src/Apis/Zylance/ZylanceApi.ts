import * as Endpoints from "./Endpoints"
import { getTransport } from "./Transports/ITransport"
import { ZylanceClient } from "./ZylanceClient"

export async function createZylanceApi () {
  const transport = await getTransport()
  const client = new ZylanceClient(transport)

  return {
    observeEvent: client.observeEvent.bind(client),

    desktop: Endpoints.createDesktopApi(client),
    status: Endpoints.createStatusApi(client),
    echo: Endpoints.createEchoApi(client),
    files: Endpoints.createFileApi(client),
    vault: Endpoints.createVaultApi(client),
    background: Endpoints.createBackgroundApi(client),
    account: Endpoints.createAccountApi(client),
    ledger: Endpoints.createLedgerApi(client),
    import: Endpoints.createImportApi(client),
    settings: Endpoints.createSettingsApi(client),
  }
}

export type ZylanceApi = Awaited<ReturnType<typeof createZylanceApi>>
