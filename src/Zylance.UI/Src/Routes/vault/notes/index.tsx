import { createFileRoute } from "@tanstack/react-router"
import type { FC } from "react"
import { NotesPanel } from "@/Components/Notes/NotesPanel"

const RouteComponent: FC = () => {
  return <NotesPanel />
}

export const Route = createFileRoute("/vault/notes/")({
  component: RouteComponent,
})
