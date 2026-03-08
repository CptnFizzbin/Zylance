import { CssBaseline } from "@mui/material"
import GlobalStyles from "@mui/material/GlobalStyles"
import { ThemeProvider } from "@mui/system"
import { StrictMode } from "react"
import ReactDOM from "react-dom/client"
import { App, TanStackQueryProviderContext } from "@/App"
import { ZylanceProvider } from "@/Components/Application/ZylanceProvider"
import { theme } from "./Integrations/mui/Theme"
import * as TanStackQueryProvider from "./Integrations/tanstack-query/root-provider"

import "./styles.css"

// Render the app
const rootElement = document.getElementById("app")

if (rootElement && !rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement)
  root.render(
    <StrictMode>
      <TanStackQueryProvider.Provider {...TanStackQueryProviderContext}>
        <ZylanceProvider>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <GlobalStyles styles="@layer theme,base,mui,components,utilities;" />

            <App />
          </ThemeProvider>
        </ZylanceProvider>
      </TanStackQueryProvider.Provider>
    </StrictMode>,
  )
}
