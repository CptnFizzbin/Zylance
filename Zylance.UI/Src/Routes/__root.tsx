import TanStackQueryDevtools from "@/Integrations/tanstack-query/devtools"
import type { ZylanceState } from "@Lib/ZylanceContext"
import { TanStackDevtools } from "@tanstack/react-devtools"
import type { QueryClient } from "@tanstack/react-query"
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router"
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools"

interface ZylanceRouterContext {
  queryClient: QueryClient
  zylance: ZylanceState
}

export const Route = createRootRouteWithContext<ZylanceRouterContext>()({
  component: RootComponent,
  beforeLoad: async ({ context, location }) => {
    const { currentVault } = context.zylance

    if (currentVault === null) {
      if (location.pathname !== "/locked/select-vault") {
        throw Route.redirect({ to: "/locked/select-vault" })
      }

      return
    }

    if (currentVault.locked) {
      if (location.pathname !== "/locked/unlock-vault") {
        throw Route.redirect({ to: "/locked/unlock-vault" })
      }

      return
    }

    if (location.pathname.startsWith("/locked/")) {
      throw Route.redirect({ to: "/vault" })
    }
  },
})

function RootComponent() {
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
