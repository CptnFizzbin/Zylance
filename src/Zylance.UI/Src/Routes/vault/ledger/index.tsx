import { createFileRoute } from "@tanstack/react-router"
import { sort } from "fast-sort"
import { useMemo } from "react"
import { useAccounts } from "@/Components/Accounts/AccountQueries"
import { useLedgerEntries } from "@/Components/Ledger/LedgerEntryQueries"
import { LedgerGrid } from "@/Components/Ledger/LedgerGrid/LedgerGrid"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent () {
  const ledgerEntriesQuery = useLedgerEntries()
  const accountsQuery = useAccounts()
  const entries = ledgerEntriesQuery.data || []
  const accounts = accountsQuery.data || []

  const sortedEntries = useMemo(() => {
    return sort(entries).by({ asc: (entry) => entry.timestamp })
  }, [entries])

  if (ledgerEntriesQuery.isPending || accountsQuery.isPending) {
    return <div>Loading...</div>
  }

  if (ledgerEntriesQuery.isError) return <div>Error loading ledger entries</div>
  if (accountsQuery.isError) return <div>Error loading accounts</div>

  return <LedgerGrid entries={sortedEntries} accounts={accounts} />
}
