import type { VaultRef } from "@Contract/models/Vault"
import { useQuery } from "@tanstack/react-query"
import { useEffect, useState } from "react"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"

export const useCurrentVault = () => {
  const zylanceApi = useZylanceApi()
  const zylanceQueries = useZylanceQueries()
  const [currentVault, setCurrentVault] = useState<VaultRef | null>(null)

  const { data: vaultStatus } = useQuery({
    ...zylanceQueries.vault.status,
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

    return () => subscriptions.forEach((unsub) => void unsub())
  }, [zylanceApi])

  return currentVault
}
