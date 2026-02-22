import { Box } from "@mui/material"
import { flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { useVirtualizer } from "@tanstack/react-virtual"
import { type FC, useEffect, useMemo, useRef } from "react"
import { ledgerGridColumns } from "@/Components/Ledger/LedgerGrid/LedgerGridColumns"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { LedgerGridRow } from "./LedgerGridRow"
import { getColumnStyle } from "./LedgerGridUtils"

const LEDGER_ROW_HEIGHT = 35

function toLedgerEntryRowData (entry: LedgerEntryData): LedgerEntryRowData {
  const amount = Number(entry.amount)
  return {
    ...entry,
    credit: amount < 0 ? entry.amount : "",
    debit: amount > 0 ? entry.amount : "",
  }
}

export interface LedgerGridProps {
  entries: LedgerEntryData[]
}

export const LedgerGrid: FC<LedgerGridProps> = ({ entries }) => {
  console.log("LedgerGrid render")
  const wrapperRef = useRef(null)

  const rowVirtualizer = useVirtualizer({
    count: entries.length,
    getScrollElement: () => wrapperRef.current,
    estimateSize: () => LEDGER_ROW_HEIGHT,
    getItemKey: (index) => entries[index].id,
    overscan: 20,
  })

  const rowsData = useMemo(() => entries.map(toLedgerEntryRowData), [entries])
  const table = useReactTable({
    data: rowsData,
    columns: ledgerGridColumns,
    getCoreRowModel: getCoreRowModel(),
  })

  useEffect(() => {
    console.log("Entries updated, scrolling to bottom")
    if (entries.length > 0) {
      rowVirtualizer.scrollToIndex(entries.length - 1)
    }
  }, [entries.length, rowVirtualizer])

  const { headers } = table.getHeaderGroups()[0]
  const { rows } = table.getRowModel()

  const onEdit = (rowData: LedgerEntryRowData) => {
    console.log("Editing row:", rowData)
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
              onEdit={onEdit}
            />
          )
        })}
      </Box>
    </Box>
  )
}
