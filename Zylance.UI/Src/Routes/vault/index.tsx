import { Box } from "@mui/material"
import { createFileRoute, Navigate, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/vault/")({
  component: RouteComponent,
})

function RouteComponent() {
  console.log("Rendering /vault route")
  const match = Route.useMatch()

  if (match) {
    return <Navigate from={Route.fullPath} to="./ledger" />
  }

  return (
    <Box flexGrow={1} overflow="auto">
      <Outlet />
    </Box>
  )
}
