import type { Cell } from "@tanstack/react-table"
import type { FC } from "react"
import type { LedgerEntryRowData } from "../UseLedgerRowForm"

export const LedgerGridCell: FC<{
  cell: Cell<LedgerEntryRowData, unknown>
}> = ({ cell }) => {
  return <>{cell.getValue()}</>
}
