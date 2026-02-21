import type * as AccountTypes from "$Contract/api/Account"
import { ZylanceActions } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createAccountApi (client: ZylanceClient) {
  return {
    listAccounts: client.createRequestEndpoint<
      typeof ZylanceActions.Account_ListAccounts,
      AccountTypes.ListAccountsReq,
      AccountTypes.ListAccountsRes
    >(ZylanceActions.Account_ListAccounts),

    getAccount: client.createRequestEndpoint<
      typeof ZylanceActions.Account_GetAccount,
      AccountTypes.GetAccountReq,
      AccountTypes.GetAccountRes
    >(ZylanceActions.Account_GetAccount),

    createAccount: client.createRequestEndpoint<
      typeof ZylanceActions.Account_CreateAccount,
      AccountTypes.CreateAccountReq,
      AccountTypes.CreateAccountRes
    >(ZylanceActions.Account_CreateAccount),

    updateAccount: client.createRequestEndpoint<
      typeof ZylanceActions.Account_UpdateAccount,
      AccountTypes.UpdateAccountReq,
      AccountTypes.UpdateAccountRes
    >(ZylanceActions.Account_UpdateAccount),

    deleteAccount: client.createRequestEndpoint<
      typeof ZylanceActions.Account_DeleteAccount,
      AccountTypes.DeleteAccountReq,
      AccountTypes.DeleteAccountRes
    >(ZylanceActions.Account_DeleteAccount),
  }
}

export type AccountApi = ReturnType<typeof createAccountApi>
