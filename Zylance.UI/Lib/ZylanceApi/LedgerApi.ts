import type * as LedgerTypes from "@Contract/api/Ledger"
import type { ZylanceClient } from "@Lib/ZylanceClient"
import { ZylanceActions } from "../../Generated/ZylanceConstants"

export function createLedgerApi(client: ZylanceClient) {
  return {
    createLedgerEntry: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_CreateLedgerEntry,
      LedgerTypes.CreateLedgerEntryReq,
      LedgerTypes.CreateLedgerEntryRes
    >(ZylanceActions.Ledger_CreateLedgerEntry),
    getLedgerEntry: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_GetLedgerEntry,
      LedgerTypes.GetLedgerEntryReq,
      LedgerTypes.GetLedgerEntryRes
    >(ZylanceActions.Ledger_GetLedgerEntry),
    listLedgerEntries: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_ListLedgerEntries,
      LedgerTypes.ListLedgerEntriesReq,
      LedgerTypes.ListLedgerEntriesRes
    >(ZylanceActions.Ledger_ListLedgerEntries),
    updateLedgerEntry: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_UpdateLedgerEntry,
      LedgerTypes.UpdateLedgerEntryReq,
      LedgerTypes.UpdateLedgerEntryRes
    >(ZylanceActions.Ledger_UpdateLedgerEntry),
    deleteLedgerEntry: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_DeleteLedgerEntry,
      LedgerTypes.DeleteLedgerEntryReq,
      LedgerTypes.DeleteLedgerEntryRes
    >(ZylanceActions.Ledger_DeleteLedgerEntry),
    searchLedgerEntries: client.createRequestEndpoint<
      typeof ZylanceActions.Ledger_SearchLedgerEntries,
      LedgerTypes.SearchLedgerEntriesReq,
      LedgerTypes.SearchLedgerEntriesRes
    >(ZylanceActions.Ledger_SearchLedgerEntries),
  }
}

export type LedgerApi = ReturnType<typeof createLedgerApi>
