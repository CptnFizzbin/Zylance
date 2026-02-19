import { Box } from "@mui/material"
import type { Row } from "@tanstack/react-table"
import { type FC, memo } from "react"
import { LedgerGridCell } from "@/Components/Ledger/LedgerGridCell"
import { type LedgerEntryRowData, useLedgerRowForm } from "@/Components/Ledger/UseLedgerRowForm"
import { getJustifyContent } from "./LedgerGridUtils"

export const LedgerGridRow: FC<{
  row: Row<LedgerEntryRowData>
}> = memo(({ row }) => {
  const form = useLedgerRowForm({ ledgerEntry: row.original })
  const onSave = () => form.handleSubmit()
  const onReset = () => form.reset()
  const numColumns = row.getVisibleCells().length

  return (
    <Box
      component={"form"}
      sx={{
        display: "grid",
        gridTemplateColumns: "subgrid",
        gridColumn: `1 / ${numColumns + 1}`,
      }}
    >
      {row.getVisibleCells().map((cell) => (
        <Box
          key={cell.id}
          sx={{
            padding: 0.5,
            borderBottom: "1px solid",
            borderColor: "divider",
            display: "flex",
            alignItems: "center",
            justifyContent: getJustifyContent(cell.column),
          }}
        >
          <LedgerGridCell
            cell={cell}
            form={form}
            onSave={onSave}
            onReset={onReset}
          />
        </Box>
      ))}
    </Box>
  )
})
