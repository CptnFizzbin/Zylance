import type * as ImportTypes from "$Contract/api/Import"
import { ZylanceActions, ZylanceEvents } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createImportApi (client: ZylanceClient) {
  return {
    uploadFile: client.createRequestEndpoint<
      typeof ZylanceActions.Import_Start,
      ImportTypes.StartImportReq,
      ImportTypes.StartImportRes
    >(ZylanceActions.Import_Start),

    setAccounts: client.createEventEmitter<
      typeof ZylanceEvents.Import_SetAccounts,
      ImportTypes.ImportSetAccountsEvt
    >(ZylanceEvents.Import_SetAccounts),

    cancelImport: client.createEventEmitter<
      typeof ZylanceEvents.Import_Cancelled,
      ImportTypes.ImportCancelledEvt
    >(ZylanceEvents.Import_Cancelled),
  }
}

export type ImportApi = ReturnType<typeof createImportApi>
