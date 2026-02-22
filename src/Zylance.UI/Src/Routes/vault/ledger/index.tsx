import { createFileRoute } from "@tanstack/react-router"
import { sort } from "fast-sort"
import { useMemo } from "react"
import { useZylance } from "@/Apis/Zylance/UseZylance"
import { useLedgerEntries } from "@/Components/Ledger/LedgerEntryQueries"
import { LedgerGrid } from "@/Components/Ledger/LedgerGrid/LedgerGrid"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent () {
  console.log("Parent render")

  const { currentVault } = useZylance()
  const ledgerEntriesQuery = useLedgerEntries(currentVault)
  const entries = ledgerEntriesQuery.data || []

  const sortedEntries = useMemo(() => {
    console.log("Entries updated, sorting entries")
    return sort(entries).by({ asc: (entry) => entry.timestamp })
  }, [entries])

  if (ledgerEntriesQuery.isPending) return <div>Loading...</div>
  if (ledgerEntriesQuery.isError) return <div>Error loading ledger entries</div>

  return <LedgerGrid entries={sortedEntries} />
}
