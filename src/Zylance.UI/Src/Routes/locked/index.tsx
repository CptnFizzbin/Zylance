import { Box } from "@mui/material"
import { createFileRoute, Outlet } from "@tanstack/react-router"

export const Route = createFileRoute("/locked/")({
  component: RouteComponent,
})

function RouteComponent() {
  console.log("Rendering /_locked route")

  return (
    <Box sx={{ flexGrow: 1, overflow: "auto" }}>
      <Outlet />
    </Box>
  )
}
