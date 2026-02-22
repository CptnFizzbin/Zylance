import { Box } from "@mui/material"
import { createFileRoute, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/locked/")({
  component: RouteComponent,
})

function RouteComponent () {
  return (
    <Box flexGrow={1} overflow="auto">
      <Outlet />
    </Box>
  )
}
