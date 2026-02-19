import { Box } from "@mui/material"
import { useQuery } from "@tanstack/react-query"
import { type ColumnDef, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { sort } from "fast-sort"
import { type FC, useMemo } from "react"
import type { LedgerEntryData } from "@/../Generated/zylance/models/Ledger"
import { LedgerGridRow } from "@/Components/Ledger/LedgerGridRow"
import { formatAsCurrency, getJustifyContent } from "@/Components/Ledger/LedgerGridUtils"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import { useZylance, useZylanceApi } from "@/Hooks/UseZylance"

const columns: ColumnDef<LedgerEntryData>[] = [
  {
    accessorKey: "timestamp",
    accessorFn: (entry) => {
      const timestamp = Number(entry.timestamp)
      return Number.isNaN(timestamp) ? "" : new Date(timestamp).toLocaleString()
    },
    header: "Date",
    size: 165,
  },
  {
    accessorKey: "payee",
    header: "Payee",
    minSize: 150,
  },
  {
    accessorKey: "memo",
    header: "Memo",
    minSize: 150,
  },
  {
    accessorKey: "debit",
    accessorFn: (entry) => {
      const value = Number(entry.amount)
      return value > 0 ? formatAsCurrency(entry.amount) : ""
    },
    header: "Debit",
    size: 80,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "credit",
    accessorFn: (entry) => {
      const value = Number(entry.amount)
      return value < 0 ? formatAsCurrency(entry.amount) : ""
    },
    header: "Credit",
    size: 80,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "amount",
    accessorFn: (entry) => formatAsCurrency(entry.amount),
    header: "Amount",
    size: 100,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "actions",
    header: "Actions",
    size: 80,
  },
]

function getGridTemplateColumns (columns: ColumnDef<LedgerEntryData>[]) {
  // Use size or minSize for each column, fallback to 1fr
  return columns
    .map((col) => {
      if (col.size) return `${col.size}px`
      if (col.minSize) return `minmax(${col.minSize}px, 1fr)`
      return "1fr"
    })
    .join(" ")
}

function toLedgerEntryRowData (entry: LedgerEntryData): LedgerEntryRowData {
  const amount = Number(entry.amount)
  return {
    ...entry,
    credit: amount < 0 ? entry.amount : "",
    debit: amount > 0 ? entry.amount : "",
  }
}

export const LedgerGrid: FC = () => {
  const api = useZylanceApi()
  const { currentVault } = useZylance()

  const {
    data: entries = [],
    isLoading,
    error,
  } = useQuery({
    queryKey: ["ledger", "entries", currentVault?.id],
    enabled: !!currentVault,
    queryFn: async () => {
      if (!currentVault) return []
      const res = await api.ledger.listLedgerEntries({
        vaultRef: { id: currentVault.id, locked: false },
      })
      return res.entries
    },
  })

  const sortedEntries = useMemo(() => {
    return sort(entries).by({ desc: (entry) => entry.timestamp })
  }, [entries])

  const table = useReactTable({
    data: sortedEntries.map(toLedgerEntryRowData),
    columns: columns as ColumnDef<LedgerEntryRowData>[],
    getCoreRowModel: getCoreRowModel(),
  })

  if (isLoading) return <div>Loading...</div>
  if (error) return <div>Error loading ledger entries</div>

  const headers = table.getHeaderGroups()[0].headers

  return (
    <Box
      sx={{
        width: "100%",
        overflowX: "auto",
        overflowY: "scroll",
        position: "relative",
        display: "grid",
        gridTemplateColumns: getGridTemplateColumns(columns),
      }}
    >
      {headers.map((header) => (
        <Box
          key={header.id}
          sx={{
            padding: 1,
            fontWeight: 600,
            position: "sticky",
            top: 0,
            backgroundColor: "background.paper",
            zIndex: 1,
            display: "flex",
            alignItems: "center",
            justifyContent: getJustifyContent(header.column),
          }}
        >
          {header.isPlaceholder
            ? null
            : flexRender(header.column.columnDef.header, header.getContext())}
        </Box>
      ))}
      {table.getRowModel().rows.map((row) => {
        return <LedgerGridRow key={row.id} row={row} />
      })}
    </Box>
  )
}
