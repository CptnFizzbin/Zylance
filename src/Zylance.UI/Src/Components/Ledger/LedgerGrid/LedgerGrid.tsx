import { Box } from "@mui/material"
import { useQuery } from "@tanstack/react-query"
import { flexRender, getCoreRowModel, type Row, useReactTable } from "@tanstack/react-table"
import { useVirtualizer } from "@tanstack/react-virtual"
import { sort } from "fast-sort"
import { type FC, useMemo, useRef } from "react"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import { useZylance, useZylanceApi } from "@/Hooks/UseZylance"
import type { LedgerEntryData } from "$Generated/zylance/models/Ledger"
import { ledgerGridColumns } from "./LedgerGridColumns"
import { LedgerGridRow } from "./LedgerGridRow"
import { getColumnStyle } from "./LedgerGridUtils"

function toLedgerEntryRowData (entry: LedgerEntryData): LedgerEntryRowData {
  const amount = Number(entry.amount)
  return {
    ...entry,
    credit: amount < 0 ? entry.amount : "",
    debit: amount > 0 ? entry.amount : "",
  }
}

export const LedgerGrid: FC = () => {
  const wrapperRef = useRef(null)

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

  const rowHeight = 35

  const rowVirtualizer = useVirtualizer({
    count: entries.length,
    getScrollElement: () => wrapperRef.current,
    estimateSize: () => rowHeight,
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

  if (isLoading) return <div>Loading...</div>
  if (error) return <div>Error loading ledger entries</div>

  const { headers } = table.getHeaderGroups()[0]
  const { rows } = table.getRowModel()

  const onEditRow = (row: Row<LedgerEntryRowData>) => {
    console.log("Edit row", row.original)
  }

  return (
    <Box
      ref={wrapperRef}
      sx={{
        overflowX: "auto",
        overflowY: "scroll",
      }}
    >
      <Box
        sx={{
          height: `calc(${rowVirtualizer.getTotalSize()}px + 50px)`,
          position: "relative",
        }}
      >
        <Box
          sx={{
            position: "sticky",
            top: 0,
            backgroundColor: "background.paper",
            zIndex: 1,
            display: "flex",
            flexDirection: "row",
          }}
        >
          {headers.map((header) => (
            <Box
              key={header.id}
              sx={{
                padding: 1,
                fontWeight: 600,
                display: "flex",
                alignItems: "center",
              }}
              style={getColumnStyle(header.column)}
            >
              {header.isPlaceholder
                ? null
                : flexRender(
                  header.column.columnDef.header,
                  header.getContext(),
                )}
            </Box>
          ))}
        </Box>
        {rowVirtualizer.getVirtualItems().map((virtualRow, index) => {
          const row = rows[virtualRow.index]
          return (
            <LedgerGridRow
              key={row.id}
              row={row}
              style={{
                height: `${virtualRow.size}px`,
                transform: `translateY(${virtualRow.start - index * virtualRow.size}px)`,
              }}
              onEdit={() => onEditRow(row)}
            />
          )
        })}
      </Box>
    </Box>
  )
}
