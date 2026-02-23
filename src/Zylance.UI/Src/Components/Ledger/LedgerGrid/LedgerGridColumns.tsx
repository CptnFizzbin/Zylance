import { Typography } from "@mui/material"
import { createColumnHelper } from "@tanstack/react-table"
import { format, parseISO } from "date-fns"
import type { AccountData } from "$Contract/models/Account"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { formatAsCurrency } from "./LedgerGridUtils"

const columnHelper = createColumnHelper<LedgerEntryData>()

export const useLedgerGridColumns = (accounts: AccountData[]) => {
  const accountMap = Object.fromEntries(
    accounts.map((account) => [account.id, account]),
  )

  return [
    columnHelper.accessor("timestamp", {
      header: "Date",
      size: 100,
      cell: (cell) => format(parseISO(cell.getValue()), "yyyy-MM-dd"),
    }),
    columnHelper.accessor("trxId", {
      header: "Online ID",
      size: 210,
      cell: (cell) => {
        return (
          <Typography variant={"caption"} sx={{ fontFamily: "monospace" }}>
            {cell.getValue()}
          </Typography>
        )
      },
    }),
    columnHelper.accessor("accountId", {
      header: "Account",
      minSize: 250,
      meta: { flexGrow: 1 },
      cell: (cell) => {
        const account = accountMap[cell.getValue()]
        return account ? account.name : "Unknown Account"
      },
    }),
    columnHelper.accessor("payee", {
      header: "Payee",
      minSize: 250,
      meta: { flexGrow: 1 },
    }),
    columnHelper.accessor("memo", {
      header: "Memo",
      minSize: 250,
      meta: { flexGrow: 1 },
    }),
    columnHelper.accessor(
      (entry) => {
        return Number(entry.amount) > 0 ? formatAsCurrency(entry.amount) : ""
      },
      {
        id: "debit",
        header: "Debit",
        size: 80,
        meta: { alignment: "right" },
      },
    ),
    columnHelper.accessor(
      (entry) => {
        return Number(entry.amount) < 0 ? formatAsCurrency(entry.amount) : ""
      },
      {
        id: "credit",
        header: "Credit",
        size: 80,
        meta: { alignment: "right" },
      },
    ),
    columnHelper.accessor("amount", {
      cell: (cell) => formatAsCurrency(cell.getValue()),
      header: "Amount",
      size: 100,
      meta: { alignment: "right" },
    }),
  ]
}
