import type * as StatusTypes from "@Contract/api/Status"
import type { ZylanceClient } from "@Lib/ZylanceClient"
import { ZylanceActions } from "../../Generated/ZylanceConstants"

export function createStatusApi(client: ZylanceClient) {
  return {
    getStatus: client.createRequestEndpoint<
      typeof ZylanceActions.Status_GetStatus,
      StatusTypes.GetStatusReq,
      StatusTypes.GetStatusRes
    >(ZylanceActions.Status_GetStatus),
  }
}

export type StatusApi = ReturnType<typeof createStatusApi>
