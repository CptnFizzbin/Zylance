import type { Column } from "@tanstack/react-table"
import type { RowData } from "@tanstack/table-core"
import type { LedgerEntryRowData } from "@/Components/Ledger/UseLedgerRowForm"

declare module "@tanstack/react-table" {
  interface ColumnMeta<TData extends RowData, TValue> {
    alignment: string
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

export function getAlignment ({
  columnDef,
}: Column<LedgerEntryRowData>): string {
  return columnDef.meta?.alignment || "left"
}

export function getJustifyContent ({
  columnDef,
}: Column<LedgerEntryRowData>): string {
  const alignment = columnDef.meta?.alignment

  switch (alignment) {
    case "left":
      return "flex-start"
    case "center":
      return "center"
    case "right":
      return "flex-end"
    default:
      return "flex-start"
  }
}
