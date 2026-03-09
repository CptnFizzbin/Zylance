import { Box } from "@mui/material"
import { flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { useVirtualizer } from "@tanstack/react-virtual"
import { type FC, useEffect, useRef, useState } from "react"
import { useLedgerGridColumns } from "@/Components/Ledger/LedgerGrid/LedgerGridColumns"
import type { AccountData } from "$Contract/models/Account"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { LedgerGridRow } from "./LedgerGridRow"
import { getColumnStyle } from "./LedgerGridUtils"
import { EditLedgerEntryDialog } from "@/Components/Ledger/Dialogs/EditLedgerEntryDialog"

const LEDGER_ROW_HEIGHT = 30

export interface LedgerGridProps {
  entries: LedgerEntryData[]
  accounts: AccountData[]
}

export const LedgerGrid: FC<LedgerGridProps> = ({ entries, accounts }) => {
  const ledgerGridColumns = useLedgerGridColumns(accounts)
  const wrapperRef = useRef(null)
  const [editingEntry, setEditingEntry] = useState<LedgerEntryData | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const rowVirtualizer = useVirtualizer({
    count: entries.length,
    getScrollElement: () => wrapperRef.current,
    estimateSize: () => LEDGER_ROW_HEIGHT,
    getItemKey: (index) => entries[index].id,
    overscan: 20,
  })

  const table = useReactTable({
    data: entries,
    columns: ledgerGridColumns,
    getCoreRowModel: getCoreRowModel(),
    initialState: {
      columnVisibility: {
        trxId: false,
      },
    },
  })

  useEffect(() => {
    if (entries.length > 0) {
      rowVirtualizer.scrollToIndex(entries.length - 1)
    }
  }, [entries.length, rowVirtualizer])

  const { headers } = table.getHeaderGroups()[0]
  const { rows } = table.getRowModel()

  const onEdit = (entry: LedgerEntryData) => {
    setEditingEntry(entry)
    setDialogOpen(true)
  }

  const onSaved = () => {
    setDialogOpen(false)
  }

  return (
    <Box
      ref={wrapperRef}
      sx={{
        overflowX: "auto",
        overflowY: "scroll",
        fontSize: "10pt",
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
                backgroundColor: "background.paper",
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

      {editingEntry && (
        <EditLedgerEntryDialog
          open={dialogOpen}
          ledgerEntry={editingEntry}
          onClose={() => setDialogOpen(false)}
          onClosed={() => setEditingEntry(null)}
          onSaved={onSaved}
        />
      )}
    </Box>
  )
}
