import { Alert } from "@mui/material"
import { useQuery } from "@tanstack/react-query"
import { createFileRoute } from "@tanstack/react-router"
import { sort } from "fast-sort"
import { useMemo } from "react"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"
import { LedgerGrid } from "@/Components/Ledger/LedgerGrid/LedgerGrid"
import { LoadingScreen } from "@/Components/UI/LoadingScreen"

export const Route = createFileRoute("/vault/ledger/")({
  component: RouteComponent,
})

function RouteComponent () {
  const zylanceQueries = useZylanceQueries()

  const entriesQuery = useQuery(zylanceQueries.ledger.list())
  const entries = entriesQuery.data || []
  const accountsQuery = useQuery(zylanceQueries.accounts.list)
  const accounts = accountsQuery.data || []

  const sortedEntries = useMemo(() => {
    return sort(entries).by({ asc: (entry) => entry.timestamp })
  }, [entries])

  const isPending = entriesQuery.isPending || accountsQuery.isPending
  if (isPending) return <LoadingScreen />

  if (entriesQuery.isError)
    return (
      <Alert severity="error">
        Error occured while fetching entries: {String(entriesQuery.error)}
      </Alert>
    )

  if (accountsQuery.isError)
    return (
      <Alert severity="error">
        Error occured while fetching accounts: {String(accountsQuery.error)}
      </Alert>
    )

  return <LedgerGrid entries={sortedEntries} accounts={accounts} />
}
