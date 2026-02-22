import { createFileRoute } from "@tanstack/react-router"
import { sort } from "fast-sort"
import { useMemo } from "react"
import { useLedgerEntries } from "@/Components/Ledger/LedgerEntryQueries"
import { LedgerGrid } from "@/Components/Ledger/LedgerGrid/LedgerGrid"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent () {
  const ledgerEntriesQuery = useLedgerEntries()
  const entries = ledgerEntriesQuery.data || []

  const sortedEntries = useMemo(() => {
    return sort(entries).by({ asc: (entry) => entry.timestamp })
  }, [entries])

  if (ledgerEntriesQuery.isPending) return <div>Loading...</div>
  if (ledgerEntriesQuery.isError) return <div>Error loading ledger entries</div>

  return <LedgerGrid entries={sortedEntries} />
}
