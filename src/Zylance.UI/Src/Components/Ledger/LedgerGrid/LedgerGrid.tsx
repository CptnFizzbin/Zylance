import { Box } from "@mui/material"
import { flexRender, type Row } from "@tanstack/react-table"
import type { FC } from "react"
import { useLedgerGrid } from "@/Components/Ledger/LedgerGrid/UseLedgerGrid"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import { LedgerGridRow } from "./LedgerGridRow"
import { getColumnStyle } from "./LedgerGridUtils"

export const LedgerGrid: FC = () => {
  const { ledgerEntriesQuery, wrapperRef, rowVirtualizer, table } =
    useLedgerGrid()

  if (ledgerEntriesQuery.isLoading) return <div>Loading...</div>
  if (ledgerEntriesQuery.error) return <div>Error loading ledger entries</div>

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
