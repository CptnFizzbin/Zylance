import { Box, Stack } from "@mui/material"
import { createFileRoute, Outlet } from "@tanstack/react-router"
import { Suspense } from "react"
import { AccountsPanel } from "@/Components/Accounts/AccountsPanel"
import { MenuRibbon } from "@/Components/MenuRibbon/MenuRibbon"
import { LoadingScreen } from "@/Components/UI/LoadingScreen"

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
      <MenuRibbon />

      <Box
        sx={{
          display: "flex",
          flexGrow: 1,
          overflow: "hidden",
        }}
      >
        <AccountsPanel />

        <Stack
          component="main"
          sx={{
            flexGrow: 1,
            overflow: "auto",
            position: "relative",
          }}
        >
          <Suspense fallback={<LoadingScreen />}>
            <Outlet />
          </Suspense>
        </Stack>
      </Box>
    </Box>
  )
}
