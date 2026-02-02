import { createFileRoute, Navigate } from "@tanstack/react-router"

export const Route = createFileRoute("/vault/")({
  component: () => <Navigate to="/vault/ledger" replace />,
})
