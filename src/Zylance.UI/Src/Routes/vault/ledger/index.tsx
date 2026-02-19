import { createFileRoute } from "@tanstack/react-router"
import { LedgerGrid } from "@/Components/Ledger/LedgerGrid"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent () {
  return <LedgerGrid />
}
