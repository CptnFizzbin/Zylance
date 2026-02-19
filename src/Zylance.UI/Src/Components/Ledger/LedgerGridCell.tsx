import RestoreIcon from "@mui/icons-material/Restore"
import SaveIcon from "@mui/icons-material/Save"
import { IconButton } from "@mui/material"
import type { Cell } from "@tanstack/react-table"
import type { FC } from "react"
import { getAlignment } from "@/Components/Ledger/LedgerGridUtils"
import type { LedgerEntryRowData, LedgerRowForm } from "@/Components/Ledger/UseLedgerRowForm"

export const LedgerGridCell: FC<{
  cell: Cell<LedgerEntryRowData, unknown>
  form: LedgerRowForm
  onSave: () => void
  onReset: () => void
}> = ({ cell, form, onSave, onReset }) => {
  switch (cell.column.id) {
    case "debit":
    case "credit":
    case "timestamp":
      return <>{cell.getValue()}</>
    case "memo":
    case "payee":
    case "amount":
      return (
        <form.AppField name={cell.column.id}>
          {(field) => (
            <field.TextField
              size="small"
              sx={{
                "& .MuiInputBase-input": {
                  textAlign: getAlignment(cell.column),
                },
              }}
            />
          )}
        </form.AppField>
      )
    case "actions":
      return (
        <form.Subscribe
          selector={(store: { isDirty: boolean }) => store.isDirty}
        >
          {(isDirty: boolean) =>
            !isDirty ? null : (
              <>
                <IconButton onClick={onSave}>
                  <SaveIcon fontSize={"small"} />
                </IconButton>
                <IconButton onClick={onReset}>
                  <RestoreIcon fontSize={"small"} />
                </IconButton>
              </>
            )
          }
        </form.Subscribe>
      )
    default:
      return <>{cell.getValue()}</>
  }
}
