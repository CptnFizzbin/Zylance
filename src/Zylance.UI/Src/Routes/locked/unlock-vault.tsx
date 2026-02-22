import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/locked/unlock-vault")({
  component: RouteComponent,
})

function RouteComponent () {
  return <div>Hello "/Vault/Unlock"!</div>
}
