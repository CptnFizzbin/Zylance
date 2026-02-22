import { Box, Stack } from "@mui/material"
import { createFileRoute, Outlet } from "@tanstack/react-router"
import { AccountsPanel } from "@/Components/AccountsPanel/AccountsPanel"
import { MenuRibbon } from "@/Components/MenuRibbon/MenuRibbon"

export const Route = createFileRoute("/vault")({
  component: RouteComponent,
})

function RouteComponent () {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        height: "100vh",
        overflow: "hidden",
      }}
    >
      {/* Top MenuRibbon spanning full width */}
      <MenuRibbon />

      {/* Main content area with AccountsPanel on left and Outlet for pages */}
      <Box
        sx={{
          display: "flex",
          flexGrow: 1,
          overflow: "hidden",
        }}
      >
        {/* Left panel with accounts list */}
        <AccountsPanel />

        {/* Main content area (Outlet) */}
        <Stack
          component="main"
          sx={{
            flexGrow: 1,
            overflow: "auto",
            position: "relative",
          }}
        >
          <Outlet />
        </Stack>
      </Box>
    </Box>
  )
}
