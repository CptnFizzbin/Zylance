import { Typography } from "@mui/material"
import type { ColumnDef } from "@tanstack/react-table"
import { format, parseISO } from "date-fns"
import type { ReactNode } from "react"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { formatAsCurrency } from "./LedgerGridUtils"

export const ledgerGridColumns: ColumnDef<LedgerEntryData, ReactNode>[] = [
  {
    accessorKey: "timestamp",
    accessorFn: (entry) => format(parseISO(entry.timestamp), "yyyy-MM-dd"),
    header: "Date",
    size: 100,
  },
  {
    accessorKey: "trxId",
    header: "Online ID",
    size: 210,
    cell: ({ getValue }) => {
      return (
        <Typography variant={"caption"} sx={{ fontFamily: "monospace" }}>
          {getValue()}
        </Typography>
      )
    },
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
      return Number(entry.amount) > 0 ? formatAsCurrency(entry.amount) : ""
    },
    header: "Debit",
    size: 80,
    meta: { alignment: "right" },
  },
  {
    accessorKey: "credit",
    accessorFn: (entry) => {
      return Number(entry.amount) < 0 ? formatAsCurrency(entry.amount) : ""
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
