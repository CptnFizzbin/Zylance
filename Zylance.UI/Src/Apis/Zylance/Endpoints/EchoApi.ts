import type * as EchoTypes from "@Contract/api/Echo"
import { ZylanceActions } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createEchoApi(client: ZylanceClient) {
  return {
    echoMessage: client.createRequestEndpoint<
      typeof ZylanceActions.Echo_EchoMessage,
      EchoTypes.EchoReq,
      EchoTypes.EchoRes
    >(ZylanceActions.Echo_EchoMessage),
  }
}

export type EchoApi = ReturnType<typeof createEchoApi>
