import type * as VaultTypes from "@Contract/api/Vault"
import { ZylanceActions, ZylanceEvents } from "$Generated/ZylanceConstants"
import type { ZylanceClient } from "../ZylanceClient"

export function createVaultApi(client: ZylanceClient) {
  return {
    // ===== Requests =====
    getStatus: client.createRequestEndpoint<
      typeof ZylanceActions.Vault_GetStatus,
      void,
      VaultTypes.VaultGetStatusRes
    >(ZylanceActions.Vault_GetStatus),

    openVault: client.createRequestEndpoint<
      typeof ZylanceActions.Vault_OpenVault,
      void,
      VaultTypes.VaultOpenRes
    >(ZylanceActions.Vault_OpenVault),

    createVault: client.createRequestEndpoint<
      typeof ZylanceActions.Vault_CreateVault,
      void,
      VaultTypes.VaultCreateRes
    >(ZylanceActions.Vault_CreateVault),

    closeVault: client.createRequestEndpoint<
      typeof ZylanceActions.Vault_CloseVault,
      void,
      VaultTypes.VaultCloseRes
    >(ZylanceActions.Vault_CloseVault),

    // ===== Events =====
    onVaultOpened: client.createEventListener<
      typeof ZylanceEvents.Vault_VaultOpened,
      VaultTypes.VaultOpenedEvt
    >(ZylanceEvents.Vault_VaultOpened),

    onVaultClosed: client.createEventListener<
      typeof ZylanceEvents.Vault_VaultClosed,
      void
    >(ZylanceEvents.Vault_VaultClosed),

    onVaultUnlocked: client.createEventListener<
      typeof ZylanceEvents.Vault_Unlocked,
      VaultTypes.VaultUnlockedEvt
    >(ZylanceEvents.Vault_Unlocked),

    onVaultLocked: client.createEventListener<
      typeof ZylanceEvents.Vault_Locked,
      VaultTypes.VaultLockedEvt
    >(ZylanceEvents.Vault_Locked),
  }
}

export type VaultApi = ReturnType<typeof createVaultApi>
