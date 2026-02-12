import type { VaultRef } from "@Contract/models/Vault"
import { LinearProgress } from "@mui/material"
import { useQuery } from "@tanstack/react-query"
import {
  createContext,
  type FC,
  type PropsWithChildren,
  useEffect,
  useMemo,
  useState,
} from "react"
import { type ZylanceApi, zylanceApiPromise } from "@/Apis/Zylance/ZylanceApi"

export interface ZylanceState {
  currentVault: VaultRef | null
  zylanceApi: ZylanceApi
}

export const ZylanceContext = createContext<ZylanceState | null>(null)

export const ZylanceProvider: FC<PropsWithChildren> = ({ children }) => {
  const [zylanceApi, setZylanceApi] = useState<ZylanceApi | null>(null)
  const [currentVault, setCurrentVault] = useState<VaultRef | null>(null)

  useEffect(() => {
    zylanceApiPromise.then(setZylanceApi).catch((err: unknown) => {
      console.error("Failed to initialize Zylance API", err)
    })
  }, [])

  useEffect(() => {
    if (!zylanceApi) return

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

    return () => subscriptions.forEach((unsub) => void unsub())
  }, [zylanceApi])

  const { data: vaultStatus } = useQuery({
    enabled: zylanceApi !== null,
    queryKey: [zylanceApi, "vault", "status"],
    queryFn: async () => {
      if (!zylanceApi) return null
      const status = await zylanceApi.vault.getStatus()
      return status.vaultRef ?? null
    },
    staleTime: Number.POSITIVE_INFINITY,
  })

  useEffect(() => {
    if (!vaultStatus) return
    setCurrentVault(vaultStatus)
  }, [vaultStatus])

  const state = useMemo(() => {
    if (!zylanceApi) return null

    return {
      currentVault,
      zylanceApi,
    }
  }, [zylanceApi, currentVault])

  if (!state) {
    return (
      <LinearProgress
        variant={"indeterminate"}
        sx={{
          position: "absolute",
          top: 0,
          left: 0,
          right: 0,
          zIndex: (theme) => theme.zIndex.appBar + 1,
          height: 3,
          backgroundColor: "transparent",
        }}
      />
    )
  }

  return (
    <ZylanceContext.Provider value={state}>{children}</ZylanceContext.Provider>
  )
}
