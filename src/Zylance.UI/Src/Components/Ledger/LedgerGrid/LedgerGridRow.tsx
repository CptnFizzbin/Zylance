import { Box } from "@mui/material"
import type { SxProps } from "@mui/system"
import { flexRender, type Row } from "@tanstack/react-table"
import type { CSSProperties, FC } from "react"
import { mergeSxProps } from "@/Integrations/mui/SxPropUtils"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { getColumnStyle } from "./LedgerGridUtils"

export interface LedgerGridRowProps {
  row: Row<LedgerEntryData>
  sx?: SxProps
  style?: CSSProperties
  onEdit: (rowData: LedgerEntryData) => void
}

export const LedgerGridRow: FC<LedgerGridRowProps> = ({
  row,
  sx,
  style,
  onEdit,
}) => {
  return (
    <Box
      sx={mergeSxProps(
        { display: "flex", flexDirection: "row" }, sx, { "&:hover .ledgerCell": { backgroundColor: "action.hover" } })}
      style={style}
      onDoubleClick={() => onEdit(row.original)}
    >
      {row.getVisibleCells().map((cell) => (
        <Box
          key={cell.id}
          className={"ledgerCell"}
          sx={{
            padding: 0.5,
            borderBottom: "1px solid",
            borderColor: "divider",
            alignItems: "center",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
            overflow: "hidden",
            cursor: "default",
          }}
          style={getColumnStyle(cell.column)}
        >
          {flexRender(cell.column.columnDef.cell, cell.getContext())}
        </Box>
      ))}
    </Box>
  )
}
