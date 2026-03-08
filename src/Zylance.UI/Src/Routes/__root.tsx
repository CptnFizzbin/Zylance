import { TanStackDevtools } from "@tanstack/react-devtools"
import type { QueryClient } from "@tanstack/react-query"
import { createRootRouteWithContext, Navigate, Outlet, useLocation } from "@tanstack/react-router"
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools"
import { useEffect } from "react"
import { useZylance } from "@/Components/Application/UseZylance"
import type { ZylanceState } from "@/Components/Application/ZylanceContext"
import TanStackQueryDevtools from "@/Integrations/tanstack-query/devtools"

interface ZylanceRouterContext {
  queryClient: QueryClient
  zylance: ZylanceState
}

export const Route = createRootRouteWithContext<ZylanceRouterContext>()({
  component: RootComponent,
})

function RootComponent () {
  const { currentVault } = useZylance()
  const location = useLocation()

  useEffect(() => {
    // Marker to notify tests that React has loaded and the router is ready.
    // Tests will wait for this log before proceeding.
    console.log("Zylance Loaded")
  }, [])

  if (currentVault === null) {
    if (location.pathname !== "/locked/select-vault") {
      return <Navigate to="/locked/select-vault" />
    }
  } else if (currentVault.locked) {
    if (location.pathname !== "/locked/unlock-vault") {
      return <Navigate to="/locked/unlock-vault" />
    }
  } else if (location.pathname.startsWith("/locked/")) {
    return <Navigate to="/vault" />
  }

  return (
    <>
      <Outlet />
      <TanStackDevtools
        config={{
          position: "bottom-right",
        }}
        plugins={[
          {
            name: "Tanstack Router",
            render: <TanStackRouterDevtoolsPanel />,
          },
          TanStackQueryDevtools,
        ]}
      />
    </>
  )
}
