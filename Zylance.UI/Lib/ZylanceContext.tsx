import type { VaultRef } from "@Contract/models/Vault"
import { createZylanceApi, type ZylanceApi } from "@Lib/ZylanceApi"
import { createContext, type FC, type PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"

export interface ZylanceState {
  currentVault: VaultRef | null
  zylanceApi: ZylanceApi,
}

const zylanceApi = createZylanceApi()

const ZylanceContext = createContext<ZylanceState>({
  currentVault: null,
  zylanceApi: zylanceApi,
})

export const useZylance = () => useContext(ZylanceContext)
export const useZylanceApi = (): ZylanceApi => useContext(ZylanceContext).zylanceApi

export const ZylanceProvider: FC<PropsWithChildren> = ({ children }) => {
  const [currentVault, setCurrentVault] = useState<VaultRef | null>(null)

  useEffect(() => {
    const onVaultChanged = (data: { vaultRef?: VaultRef }) => {
      if (!data.vaultRef) return
      setCurrentVault(data.vaultRef)
    }

    const subscriptions = [
      zylanceApi.vault.onVaultOpened(onVaultChanged),
      zylanceApi.vault.onVaultUnlocked(onVaultChanged),
      zylanceApi.vault.onVaultLocked(onVaultChanged),
      zylanceApi.vault.onVaultClosed(() => setCurrentVault(null)),
    ]

    return () => subscriptions.forEach(unsub => unsub())
  }, [zylanceApi])

  const state = useMemo(() => ({
    currentVault,
    zylanceApi,
  }), [currentVault, zylanceApi])

  return (
    <ZylanceContext.Provider value={state}>
      {children}
    </ZylanceContext.Provider>
  )
}
