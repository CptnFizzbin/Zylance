import { createRootRouteWithContext, Outlet } from "@tanstack/react-router"
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools"
import { TanStackDevtools } from "@tanstack/react-devtools"

import Header from "../Components/Header"

import TanStackQueryDevtools from "../Integrations/tanstack-query/devtools"

import type { QueryClient } from "@tanstack/react-query"
import { useIsDesktop } from "@/Hooks/UseRuntime.ts"
import { DesktopMenuBar } from "@/Components/Desktop/FileBar/DesktopMenuBar.tsx"

interface MyRouterContext {
  queryClient: QueryClient
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
  component: () => {
    const isDesktop = useIsDesktop()

    return (
      <>
        {isDesktop && <DesktopMenuBar />}
        <Header />
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
  },
})
