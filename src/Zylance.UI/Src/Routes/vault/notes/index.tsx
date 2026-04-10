import { createFileRoute } from "@tanstack/react-router"
import { NotesPanel } from "@/Components/Notes/NotesPanel"

export const Route = createFileRoute("/vault/notes/")({
  component: RouteComponent,
})

function RouteComponent() {
  return <NotesPanel />
}
