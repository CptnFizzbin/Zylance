import { DesktopMenuBar } from "@/Components/Desktop/FileBar/DesktopMenuBar.tsx"
import { useIsDesktop } from "@/Hooks/UseRuntime.ts"
import { Box, Stack } from "@mui/material"
import { TanStackDevtools } from "@tanstack/react-devtools"

import type { QueryClient } from "@tanstack/react-query"
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router"
import { TanStackRouterDevtoolsPanel } from "@tanstack/react-router-devtools"

import TanStackQueryDevtools from "../Integrations/tanstack-query/devtools"

interface MyRouterContext {
  queryClient: QueryClient
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
  component: () => {
    const isDesktop = useIsDesktop()

    return (
      <>
        <Stack height={"100vh"}>
          {isDesktop && <DesktopMenuBar />}
          <Box flexGrow={1} overflow="auto">
            <Outlet />
          </Box>
        </Stack>
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
