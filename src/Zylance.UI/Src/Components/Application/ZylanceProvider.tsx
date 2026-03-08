import { LinearProgress } from "@mui/material"
import type { FC, PropsWithChildren } from "react"
import { useMemo } from "react"
import { ZylanceContext, type ZylanceState } from "@/Components/Application/ZylanceContext"
import { useUserPreferences } from "@/Components/Settings/SettingsQueries"
import { useCurrentVault } from "@/Components/Vault/UseCurrentVault"

export const ZylanceProvider: FC<PropsWithChildren> = ({ children }) => {
  const currentVault = useCurrentVault()
  const userPreferencesQuery = useUserPreferences()
  const userPreferences = userPreferencesQuery.data?.preferences

  const state = useMemo(() => {
    if (!userPreferences) return null

    return {
      currentVault,
      settings: {
        userPreferences,
      },
    } satisfies ZylanceState
  }, [currentVault, userPreferences])

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
