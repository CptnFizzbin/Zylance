import type { Cell } from "@tanstack/react-table"
import type { FC } from "react"
import type { LedgerEntryData } from "$Contract/models/Ledger"

export const LedgerGridCell: FC<{
  cell: Cell<LedgerEntryData, unknown>
}> = ({ cell }) => {
  return <>{cell.getValue()}</>
}
