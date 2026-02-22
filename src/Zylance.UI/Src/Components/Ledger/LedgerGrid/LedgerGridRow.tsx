import { Box } from "@mui/material"
import type { SxProps } from "@mui/system"
import type { Row } from "@tanstack/react-table"
import type { FC } from "react"
import { LedgerGridCell } from "@/Components/Ledger/LedgerGrid/LedgerGridCell"
import { mergeSxProps } from "@/Integrations/mui/SxPropUtils"
import type { LedgerEntryRowData } from "../UseLedgerRowForm"
import { getColumnStyle } from "./LedgerGridUtils"

export interface LedgerGridRowProps {
  row: Row<LedgerEntryRowData>
  sx?: SxProps
  style?: React.CSSProperties
  onEdit: (rowData: LedgerEntryRowData) => void
}

export const LedgerGridRow: FC<LedgerGridRowProps> = ({
  row,
  sx,
  style,
  onEdit,
}) => {
  return (
    <Box
      sx={mergeSxProps({ display: "flex", flexDirection: "row" }, sx)}
      style={style}
      onDoubleClick={() => onEdit(row.original)}
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
          <LedgerGridCell cell={cell} />
        </Box>
      ))}
    </Box>
  )
}
