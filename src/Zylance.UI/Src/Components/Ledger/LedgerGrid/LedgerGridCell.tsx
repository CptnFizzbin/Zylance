import EditIcon from "@mui/icons-material/Edit"
import type { Cell } from "@tanstack/react-table"
import type { FC } from "react"
import type { LedgerEntryRowData } from "../UseLedgerRowForm"

export const LedgerGridCell: FC<{
  cell: Cell<LedgerEntryRowData, unknown>
  onEdit: () => void
}> = ({ cell, onEdit }) => {
  switch (cell.column.id) {
    case "actions":
      return (
        <EditIcon
          fontSize={"small"}
          onClick={onEdit}
          sx={{ cursor: "pointer" }}
        />
      )
    default:
      return <>{cell.getValue()}</>
  }
}
