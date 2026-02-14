import { ZylanceEvents } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createDesktopApi(client: ZylanceClient) {
  return {
    emitExit: client.createEventEmitter(ZylanceEvents.Desktop_Exit),
  }
}

export type DesktopApi = ReturnType<typeof createDesktopApi>
