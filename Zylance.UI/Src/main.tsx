import { ZylanceProvider } from "@Lib/ZylanceContext"
import { CssBaseline } from "@mui/material"
import GlobalStyles from "@mui/material/GlobalStyles"
import { ThemeProvider } from "@mui/system"
import { createRouter, RouterProvider } from "@tanstack/react-router"
import { StrictMode } from "react"
import ReactDOM from "react-dom/client"
import { theme } from "./Integrations/mui/Theme"
import * as TanStackQueryProvider from "./Integrations/tanstack-query/root-provider"
import { routeTree } from "./routeTree.gen"

import "./styles.css"

const TanStackQueryProviderContext = TanStackQueryProvider.getContext()
const router = createRouter({
  routeTree,
  context: {
    ...TanStackQueryProviderContext,
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

// Render the app
const rootElement = document.getElementById("app")

if (rootElement && !rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement)
  root.render(
    <StrictMode>
      <ZylanceProvider>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <GlobalStyles styles="@layer theme,base,mui,components,utilities;" />

          <TanStackQueryProvider.Provider {...TanStackQueryProviderContext}>
            <RouterProvider router={router} />
          </TanStackQueryProvider.Provider>
        </ThemeProvider>
      </ZylanceProvider>
    </StrictMode>,
  )
}
