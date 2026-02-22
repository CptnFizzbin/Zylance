import type { ColumnDef } from "@tanstack/react-table"
import type { LedgerEntryRowData } from "../UseLedgerRowForm"
import { formatAsCurrency } from "./LedgerGridUtils"

export const ledgerGridColumns: ColumnDef<LedgerEntryRowData>[] = [
  {
    accessorKey: "timestamp",
    accessorFn: (entry) => {
      const timestamp = Number(entry.timestamp)
      return Number.isNaN(timestamp) ? "" : new Date(timestamp).toLocaleString()
    },
    header: "Date",
    size: 165,
  },
  {
    accessorKey: "payee",
    header: "Payee",
    minSize: 250,
    meta: { flexGrow: 1 },
  },
  {
    accessorKey: "memo",
    header: "Memo",
    minSize: 250,
    meta: { flexGrow: 1 },
  },
  {
    accessorKey: "debit",
    accessorFn: (entry) => {
      return entry.debit ? formatAsCurrency(entry.debit) : ""
    },
    header: "Debit",
    size: 80,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "credit",
    accessorFn: (entry) => {
      return entry.credit ? formatAsCurrency(entry.credit) : ""
    },
    header: "Credit",
    size: 80,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "amount",
    accessorFn: (entry) => formatAsCurrency(entry.amount),
    header: "Amount",
    size: 100,
    meta: { alignment: "right" },
  },
]
