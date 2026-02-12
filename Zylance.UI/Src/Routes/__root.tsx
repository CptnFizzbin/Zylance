import { TanStackDevtools } from "@tanstack/react-devtools"
import type { QueryClient } from "@tanstack/react-query"
import {
  createRootRouteWithContext,
  Navigate,
  Outlet,
  useLocation,
} from "@tanstack/react-router"
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools"
import type { ZylanceState } from "@/Contexts/ZylanceContext"
import { useZylance } from "@/Hooks/UseZylance"
import TanStackQueryDevtools from "@/Integrations/tanstack-query/devtools"

interface ZylanceRouterContext {
  queryClient: QueryClient
  zylance: ZylanceState
}

export const Route = createRootRouteWithContext<ZylanceRouterContext>()({
  component: RootComponent,
})

function RootComponent() {
  const { currentVault } = useZylance()
  const location = useLocation()

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
