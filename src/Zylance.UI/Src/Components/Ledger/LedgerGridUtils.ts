import type { Column } from "@tanstack/react-table"
import type { RowData } from "@tanstack/table-core"
import type { CSSProperties } from "react"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"

declare module "@tanstack/react-table" {
  interface ColumnMeta<TData extends RowData, TValue> {
    alignment?: "left" | "center" | "right"
    flexGrow?: number
  }
}

const currencyFormatter = new Intl.NumberFormat("en-NA", {
  style: "currency",
  currency: "USD",
  currencyDisplay: "narrowSymbol",
})

export function formatAsCurrency (value: string | number): string {
  if (typeof value === "string") {
    value = Number(value)
    if (Number.isNaN(value)) return String(value)
  }

  return currencyFormatter.format(value)
}

export function getColumnStyle (
  column: Column<LedgerEntryRowData>,
): CSSProperties {
  const { columnDef } = column

  return {
    width: columnDef.size,
    minWidth: columnDef.minSize,
    maxWidth: columnDef.maxSize,
    flexGrow: columnDef.meta?.flexGrow,
    flexShrink: 0,
    textAlign: columnDef.meta?.alignment,
  }
}
