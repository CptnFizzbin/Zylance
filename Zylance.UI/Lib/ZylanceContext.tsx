import type { VaultRef } from "@Contract/models/Vault"
import { createZylanceApi, type ZylanceApi } from "@Lib/ZylanceApi"
import { useQuery } from "@tanstack/react-query"
import { createContext, type FC, type PropsWithChildren, useContext, useEffect, useMemo, useState } from "react"

export interface ZylanceState {
  currentVault: VaultRef | null
  zylanceApi: ZylanceApi,
}

const zylanceApi = createZylanceApi()
export const ZylanceContext = createContext<ZylanceState | null>(null)

export const useZylance = () => {
  const zylance = useContext(ZylanceContext)

  if (!zylance) {
    throw new Error("useZylance must be used within a ZylanceProvider")
  }

  return zylance
}

export const useZylanceApi = (): ZylanceApi => useZylance().zylanceApi

export const ZylanceProvider: FC<PropsWithChildren> = ({ children }) => {
  const [currentVault, setCurrentVault] = useState<VaultRef | null>(null)

  const { data: vaultStatus } = useQuery({
    queryKey: ["vault", "status"],
    queryFn: async () => {
      const status = await zylanceApi.vault.getStatus()
      return status.vaultRef ?? null
    },
    staleTime: Number.POSITIVE_INFINITY,
  })

  useEffect(() => {
    if (!vaultStatus) return
    setCurrentVault(vaultStatus)
  }, [vaultStatus])

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
