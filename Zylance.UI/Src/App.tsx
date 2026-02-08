import { useZylance } from "@Lib/ZylanceContext"
import { Stack } from "@mui/material"
import { createRouter, RouterProvider } from "@tanstack/react-router"
import type { FC } from "react"
import { BackgroundProgressBar } from "@/Components/Background/BackgroundProgressBar"
import { DesktopMenuBar } from "@/Components/Desktop/FileBar/DesktopMenuBar"
import { useIsDesktop } from "@/Components/Runtime/Hooks/UseRuntime"
import * as TanStackQueryProvider from "@/Integrations/tanstack-query/root-provider"
import { routeTree } from "@/routeTree.gen"

export const TanStackQueryProviderContext = TanStackQueryProvider.getContext()

const router = createRouter({
  routeTree,
  context: {
    ...TanStackQueryProviderContext,
    // biome-ignore lint/suspicious/noExplicitAny: Will be provided in App component
    zylance: undefined as any,
  },
  defaultPreload: "intent",
  scrollRestoration: true,
  defaultStructuralSharing: true,
  defaultPreloadStaleTime: 0,
})

// Register the router instance for type safety
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router
  }
}

export const App: FC = () => {
  const isDesktop = useIsDesktop()
  const zylance = useZylance()

  return (
    <Stack height={"100vh"}>
      {isDesktop && <DesktopMenuBar />}
      <Stack flexGrow={1} overflow="auto" sx={{ position: "relative" }}>
        <BackgroundProgressBar />

        <RouterProvider router={router} context={{ zylance }} />
      </Stack>
    </Stack>
  )
}
