import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent() {
  console.log("Rendering /_unlocked/ledger route")

  return <div>Hello "/Ledger/Index"!</div>
}
