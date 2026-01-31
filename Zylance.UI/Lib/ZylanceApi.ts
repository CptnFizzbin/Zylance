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
import type { GetLedgerReq, GetLedgerRes } from "@Contract/api/Ledger"
import type { GetStatusReq, GetStatusRes } from "@Contract/api/Status"
import type { VaultCreateReq, VaultCreateRes, VaultOpenRes } from "@Contract/api/Vault"
import { ZylanceClient } from "@Lib/ZylanceClient"

export function createZylanceApi () {
  const client = new ZylanceClient()

  return {
    desktop: {
      emitExit: client.createEventEndpoint<"Desktop:Exit">("Desktop:Exit"),
    },

    status: {
      getStatus: client.createRequestEndpoint<"Status:GetStatus", GetStatusReq, GetStatusRes>("Status:GetStatus"),
    },

    echo: {
      echoMessage: client.createRequestEndpoint<"Echo:EchoMessage", EchoReq, EchoRes>("Echo:EchoMessage"),
    },

    files: {
      selectFile: client.createRequestEndpoint<"File:SelectFile", SelectFileReq, SelectFileRes>("File:SelectFile"),
      createFile: client.createRequestEndpoint<"File:CreateFile", CreateFileReq, CreateFileRes>("File:CreateFile"),
      saveFile: client.createRequestEndpoint<"File:SaveFile", SaveFileReq, void>("File:SaveFile"),
      getFile: client.createRequestEndpoint<"File:GetFile", GetFileReq, FileContentRes>("File:GetFile"),
    },

    vault: {
      openVault: client.createRequestEndpoint<"Vault:OpenVault", void, VaultOpenRes>("Vault:OpenVault"),
      createVault: client.createRequestEndpoint<"Vault:CreateVault", VaultCreateReq, VaultCreateRes>(
        "Vault:CreateVault"),
    },

    account: {
      listAccounts: client.createRequestEndpoint<"Account:ListAccounts", ListAccountsReq, ListAccountsRes>(
        "Account:ListAccounts"),
      getAccount: client.createRequestEndpoint<"Account:GetAccount", GetAccountReq, GetAccountRes>(
        "Account:GetAccount"),
      createAccount: client.createRequestEndpoint<"Account:CreateAccount", CreateAccountReq, CreateAccountRes>(
        "Account:CreateAccount"),
      updateAccount: client.createRequestEndpoint<"Account:UpdateAccount", UpdateAccountReq, UpdateAccountRes>(
        "Account:UpdateAccount"),
      deleteAccount: client.createRequestEndpoint<"Account:DeleteAccount", DeleteAccountReq, DeleteAccountRes>(
        "Account:DeleteAccount"),
    },

    ledger: {
      getLedger: client.createRequestEndpoint<"Ledger:GetLedger", GetLedgerReq, GetLedgerRes>("Ledger:GetLedger"),
    },
  }
}
