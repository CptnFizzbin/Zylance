import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/locked/unlock-vault")({
  component: RouteComponent,
})

function RouteComponent() {
  console.log("Rendering /_locked/vault/unlock route")

  return <div>Hello "/Vault/Unlock"!</div>
}
