import { getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { useVirtualizer } from "@tanstack/react-virtual"
import { sort } from "fast-sort"
import { useEffect, useMemo, useRef } from "react"
import { useZylance } from "@/Components/Application/UseZylance"
import { useLedgerEntries } from "@/Components/Ledger/LedgerEntryQueries"
import { ledgerGridColumns } from "@/Components/Ledger/LedgerGrid/LedgerGridColumns"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import type { LedgerEntryData } from "$Contract/models/Ledger"

const LEDGER_ROW_HEIGHT = 35

export const useLedgerGrid = () => {
  const wrapperRef = useRef(null)

  const { currentVault } = useZylance()
  const ledgerEntriesQuery = useLedgerEntries(currentVault)
  const entries = ledgerEntriesQuery.data || []

  const rowVirtualizer = useVirtualizer({
    count: entries.length,
    getScrollElement: () => wrapperRef.current,
    estimateSize: () => LEDGER_ROW_HEIGHT,
    overscan: 20,
  })

  const sortedEntries = useMemo(() => {
    return sort(entries).by({ desc: (entry) => entry.timestamp })
  }, [entries])

  const table = useReactTable({
    data: sortedEntries.map(toLedgerEntryRowData),
    columns: ledgerGridColumns,
    getCoreRowModel: getCoreRowModel(),
  })

  useEffect(() => {
    if (entries.length > 0) {
      rowVirtualizer.scrollToIndex(entries.length - 1)
    }
  }, [entries, rowVirtualizer])

  return {
    ledgerEntriesQuery,
    wrapperRef,
    rowVirtualizer,
    table,
  }
}

function toLedgerEntryRowData (entry: LedgerEntryData): LedgerEntryRowData {
  const amount = Number(entry.amount)
  return {
    ...entry,
    credit: amount < 0 ? entry.amount : "",
    debit: amount > 0 ? entry.amount : "",
  }
}
