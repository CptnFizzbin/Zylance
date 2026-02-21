import { createContext } from "react"
import type { UserPreferencesData } from "$Contract/api/Settings"
import type { VaultRef } from "$Contract/models/Vault"

export interface ZylanceSettingsState {
  userPreferences: UserPreferencesData
}

export interface ZylanceState {
  currentVault: VaultRef | null
  settings: ZylanceSettingsState
}

export const ZylanceContext = createContext<ZylanceState | null>(null)
