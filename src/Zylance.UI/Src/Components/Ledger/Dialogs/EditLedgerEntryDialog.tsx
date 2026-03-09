import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from "@mui/material"
import type { FC } from "react"
import { useLedgerRowForm } from "@/Components/Ledger/UseLedgerRowForm"
import type { LedgerEntryData } from "$Contract/models/Ledger"
import { AccountSelectField } from "@/Components/Accounts/AccountSelectField"

export interface EditLedgerEntryDialogProps {
  open: boolean
  ledgerEntry: LedgerEntryData
  onClose: () => void
  onClosed?: () => void
  onSaved?: () => void
}

export const EditLedgerEntryDialog: FC<EditLedgerEntryDialogProps> = ({
  open,
  ledgerEntry,
  onClose,
  onClosed,
  onSaved,
}) => {
  const form = useLedgerRowForm({ ledgerEntry })

  const handleSave = async () => {
    try {
      await form.handleSubmit()
      onSaved?.()
      onClose()
    } catch (err) {
      console.error("Failed to save ledger entry:", err)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm" onTransitionExited={onClosed}>
      <DialogTitle>Edit Ledger Entry</DialogTitle>

      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <form.AppField name={"trxId"}>
            {(field) => <field.TextField label={"Transaction ID"} disabled />}
          </form.AppField>

          <form.AppField name={"accountId"}>
            {() => <AccountSelectField label={"Account"} placeholder={"Select account"} />}
          </form.AppField>

          <form.AppField name={"payee"}>
            {(field) => <field.TextField label={"Payee"} />}
          </form.AppField>

          <form.AppField name={"memo"}>
            {(field) => <field.TextField label={"Memo"} />}
          </form.AppField>

          <form.AppField name={"amount"}>
            {(field) => (
              <form.Subscribe selector={state => state.values.accountId}>
                {(accountId) => (
                  <field.CurrencyField label={"Amount"} type={"number"} accountId={accountId} />
                )}
              </form.Subscribe>
            )}
          </form.AppField>
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSave}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  )
}

