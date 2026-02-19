import { Box } from "@mui/material"
import type { SxProps } from "@mui/system"
import type { Row } from "@tanstack/react-table"
import { type CSSProperties, type FC, memo } from "react"
import { LedgerGridCell } from "@/Components/Ledger/LedgerGridCell"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"
import { mergeSxProps } from "@/Integrations/mui/SxPropUtils"
import { getColumnStyle } from "./LedgerGridUtils"

export const LedgerGridRow: FC<{
  row: Row<LedgerEntryRowData>
  sx?: SxProps
  style?: CSSProperties
  onEdit: () => void
}> = memo(({ row, sx, style, onEdit }) => {
  return (
    <Box
      sx={mergeSxProps({ display: "flex", flexDirection: "row" }, sx)}
      style={style}
    >
      {row.getVisibleCells().map((cell) => (
        <Box
          key={cell.id}
          sx={{
            padding: 0.5,
            borderBottom: "1px solid",
            borderColor: "divider",
            alignItems: "center",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
            overflow: "hidden",
          }}
          style={getColumnStyle(cell.column)}
        >
          <LedgerGridCell cell={cell} onEdit={onEdit} />
        </Box>
      ))}
    </Box>
  )
})
