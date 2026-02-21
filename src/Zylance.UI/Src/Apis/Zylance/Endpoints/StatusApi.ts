import type * as StatusTypes from "$Contract/api/Status"
import { ZylanceActions } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createStatusApi (client: ZylanceClient) {
  return {
    getStatus: client.createRequestEndpoint<
      typeof ZylanceActions.Status_GetStatus,
      StatusTypes.GetStatusReq,
      StatusTypes.GetStatusRes
    >(ZylanceActions.Status_GetStatus),
  }
}

export type StatusApi = ReturnType<typeof createStatusApi>
