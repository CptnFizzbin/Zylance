import type * as EchoTypes from "@Contract/api/Echo"
import type { ZylanceClient } from "@Lib/ZylanceClient"
import { ZylanceActions } from "../../Generated/ZylanceConstants"

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
