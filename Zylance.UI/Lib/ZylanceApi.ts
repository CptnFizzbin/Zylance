import type {
  CreateAccountReq,
  CreateAccountRes,
  DeleteAccountReq,
  DeleteAccountRes,
  GetAccountReq,
  GetAccountRes,
  ListAccountsReq,
  ListAccountsRes,
  UpdateAccountReq,
  UpdateAccountRes,
} from "@Contract/api/Account"
import type {
  BackgroundWorkFinishEvt,
  BackgroundWorkProgressEvt,
  BackgroundWorkStartEvt,
} from "@Contract/api/Background"
import type { EchoReq, EchoRes } from "@Contract/api/Echo"
import type {
  CreateFileReq,
  CreateFileRes,
  FileContentRes,
  GetFileReq,
  SaveFileReq,
  SelectFileReq,
  SelectFileRes,
} from "@Contract/api/File"
import type {
  CreateLedgerEntryReq,
  CreateLedgerEntryRes,
  DeleteLedgerEntryReq,
  DeleteLedgerEntryRes,
  GetLedgerEntryReq,
  GetLedgerEntryRes,
  ListLedgerEntriesReq,
  ListLedgerEntriesRes,
  SearchLedgerEntriesReq,
  SearchLedgerEntriesRes,
  UpdateLedgerEntryReq,
  UpdateLedgerEntryRes,
} from "@Contract/api/Ledger"
import type { GetStatusReq, GetStatusRes } from "@Contract/api/Status"
import type {
  VaultCloseRes,
  VaultCreateRes,
  VaultGetStatusRes,
  VaultLockedEvt,
  VaultOpenedEvt,
  VaultOpenRes,
  VaultUnlockedEvt,
} from "@Contract/api/Vault"
import { ZylanceClient } from "@Lib/ZylanceClient"
import { ZylanceActions, ZylanceEvents } from "../Generated/ZylanceConstants"

export function createZylanceApi () {
  const client = new ZylanceClient()

  return {
    desktop: {
      emitExit: client.createEventEmitter(ZylanceEvents.Desktop_Exit),
    },

    status: {
      getStatus: client.createRequestEndpoint<typeof ZylanceActions.Status_GetStatus, GetStatusReq, GetStatusRes>(ZylanceActions.Status_GetStatus),
    },

    echo: {
      echoMessage: client.createRequestEndpoint<typeof ZylanceActions.Echo_EchoMessage, EchoReq, EchoRes>(ZylanceActions.Echo_EchoMessage),
    },

    files: {
      selectFile: client.createRequestEndpoint<typeof ZylanceActions.File_SelectFile, SelectFileReq, SelectFileRes>(ZylanceActions.File_SelectFile),
      createFile: client.createRequestEndpoint<typeof ZylanceActions.File_CreateFile, CreateFileReq, CreateFileRes>(ZylanceActions.File_CreateFile),
      saveFile: client.createRequestEndpoint<typeof ZylanceActions.File_SaveFile, SaveFileReq, void>(ZylanceActions.File_SaveFile),
      getFile: client.createRequestEndpoint<typeof ZylanceActions.File_GetFile, GetFileReq, FileContentRes>(ZylanceActions.File_GetFile),
    },

    vault: {
      getStatus: client.createRequestEndpoint<typeof ZylanceActions.Vault_GetStatus, void, VaultGetStatusRes>(ZylanceActions.Vault_GetStatus),
      openVault: client.createRequestEndpoint<typeof ZylanceActions.Vault_OpenVault, void, VaultOpenRes>(ZylanceActions.Vault_OpenVault),
      createVault: client.createRequestEndpoint<typeof ZylanceActions.Vault_CreateVault, void, VaultCreateRes>(
        ZylanceActions.Vault_CreateVault),
      closeVault: client.createRequestEndpoint<typeof ZylanceActions.Vault_CloseVault, void, VaultCloseRes>(ZylanceActions.Vault_CloseVault),
      onVaultOpened: client.createEventListener<typeof ZylanceEvents.Vault_VaultOpened, VaultOpenedEvt>(ZylanceEvents.Vault_VaultOpened),
      onVaultClosed: client.createEventListener<typeof ZylanceEvents.Vault_VaultClosed, void>(ZylanceEvents.Vault_VaultClosed),
      onVaultUnlocked: client.createEventListener<typeof ZylanceEvents.Vault_Unlocked, VaultUnlockedEvt>(ZylanceEvents.Vault_Unlocked),
      onVaultLocked: client.createEventListener<typeof ZylanceEvents.Vault_Locked, VaultLockedEvt>(ZylanceEvents.Vault_Locked),
    },

    background: {
      onWorkStart: client.createEventListener<typeof ZylanceEvents.Background_WorkStart, BackgroundWorkStartEvt>(ZylanceEvents.Background_WorkStart),
      onWorkProgress: client.createEventListener<typeof ZylanceEvents.Background_WorkProgress, BackgroundWorkProgressEvt>(ZylanceEvents.Background_WorkProgress),
      onWorkFinish: client.createEventListener<typeof ZylanceEvents.Background_WorkFinish, BackgroundWorkFinishEvt>(ZylanceEvents.Background_WorkFinish),
    },

    account: {
      listAccounts: client.createRequestEndpoint<typeof ZylanceActions.Account_ListAccounts, ListAccountsReq, ListAccountsRes>(
        ZylanceActions.Account_ListAccounts),
      getAccount: client.createRequestEndpoint<typeof ZylanceActions.Account_GetAccount, GetAccountReq, GetAccountRes>(
        ZylanceActions.Account_GetAccount),
      createAccount: client.createRequestEndpoint<typeof ZylanceActions.Account_CreateAccount, CreateAccountReq, CreateAccountRes>(
        ZylanceActions.Account_CreateAccount),
      updateAccount: client.createRequestEndpoint<typeof ZylanceActions.Account_UpdateAccount, UpdateAccountReq, UpdateAccountRes>(
        ZylanceActions.Account_UpdateAccount),
      deleteAccount: client.createRequestEndpoint<typeof ZylanceActions.Account_DeleteAccount, DeleteAccountReq, DeleteAccountRes>(
        ZylanceActions.Account_DeleteAccount),
    },

    ledger: {
      createLedgerEntry: client.createRequestEndpoint<typeof ZylanceActions.Ledger_CreateLedgerEntry, CreateLedgerEntryReq, CreateLedgerEntryRes>(
        ZylanceActions.Ledger_CreateLedgerEntry),
      getLedgerEntry: client.createRequestEndpoint<typeof ZylanceActions.Ledger_GetLedgerEntry, GetLedgerEntryReq, GetLedgerEntryRes>(
        ZylanceActions.Ledger_GetLedgerEntry),
      listLedgerEntries: client.createRequestEndpoint<typeof ZylanceActions.Ledger_ListLedgerEntries, ListLedgerEntriesReq, ListLedgerEntriesRes>(
        ZylanceActions.Ledger_ListLedgerEntries),
      updateLedgerEntry: client.createRequestEndpoint<typeof ZylanceActions.Ledger_UpdateLedgerEntry, UpdateLedgerEntryReq, UpdateLedgerEntryRes>(
        ZylanceActions.Ledger_UpdateLedgerEntry),
      deleteLedgerEntry: client.createRequestEndpoint<typeof ZylanceActions.Ledger_DeleteLedgerEntry, DeleteLedgerEntryReq, DeleteLedgerEntryRes>(
        ZylanceActions.Ledger_DeleteLedgerEntry),
      searchLedgerEntries: client.createRequestEndpoint<typeof ZylanceActions.Ledger_SearchLedgerEntries, SearchLedgerEntriesReq, SearchLedgerEntriesRes>(
        ZylanceActions.Ledger_SearchLedgerEntries),
    },
  }
}

export type ZylanceApi = ReturnType<typeof createZylanceApi>;
