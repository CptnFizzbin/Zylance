import type { ZylanceClient } from "@Lib/ZylanceClient"
import { ZylanceEvents } from "../../Generated/ZylanceConstants"

export function createDesktopApi(client: ZylanceClient) {
  return {
    emitExit: client.createEventEmitter(ZylanceEvents.Desktop_Exit),
  }
}

export type DesktopApi = ReturnType<typeof createDesktopApi>
