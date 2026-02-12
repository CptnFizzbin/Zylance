import { getTransport } from "@/Apis/Zylance/Transports/ITransport"
import { ZylanceClient } from "@/Apis/Zylance/ZylanceClient"
import * as Endpoints from "./Endpoints"

export async function createZylanceApi() {
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
  }
}

export const zylanceApiPromise = createZylanceApi()

export type ZylanceApi = Awaited<typeof zylanceApiPromise>
