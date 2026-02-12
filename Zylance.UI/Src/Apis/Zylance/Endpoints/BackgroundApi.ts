import type * as BackgroundTypes from "@Contract/api/Background"
import { ZylanceEvents } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createBackgroundApi(client: ZylanceClient) {
  return {
    onWorkStart: client.createEventListener<
      typeof ZylanceEvents.Background_WorkStart,
      BackgroundTypes.BackgroundWorkStartEvt
    >(ZylanceEvents.Background_WorkStart),

    onWorkProgress: client.createEventListener<
      typeof ZylanceEvents.Background_WorkProgress,
      BackgroundTypes.BackgroundWorkProgressEvt
    >(ZylanceEvents.Background_WorkProgress),

    onWorkFinish: client.createEventListener<
      typeof ZylanceEvents.Background_WorkFinish,
      BackgroundTypes.BackgroundWorkFinishEvt
    >(ZylanceEvents.Background_WorkFinish),
  }
}

export type BackgroundApi = ReturnType<typeof createBackgroundApi>
