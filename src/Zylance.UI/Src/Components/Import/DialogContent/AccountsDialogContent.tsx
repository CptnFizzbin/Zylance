import { Button, DialogActions, DialogContent, Paper, Stack, Typography } from "@mui/material"
import { useStore } from "@tanstack/react-form"
import type { FC } from "react"
import { useImportService } from "@/Components/Import/ImportContext"

export const AccountsDialogContent: FC = () => {
  const { form, cancelImport, setAccounts } = useImportService()
  const accounts = useStore(form.store, (state) => state.values.accounts)

  const onImportBtnClick = () => {
    setAccounts(accounts)
  }

  return (
    <>
      <DialogContent>
        <Typography gutterBottom>Confirm which accounts to import:</Typography>
        <Stack spacing={1}>
          {accounts.length === 0 && (
            <Typography color="text.secondary">
              No accounts found in import.
            </Typography>
          )}
          {accounts.map((account, i) => (
            <Paper key={account.id} sx={{ padding: 2 }} variant={"outlined"}>
              <Typography>{account.id}</Typography>
              <form.AppField name={`accounts[${i}].name`}>
                {(subField) => <subField.TextField label={"Name"} />}
              </form.AppField>
              <form.AppField name={`accounts[${i}].type`}>
                {(subField) => <subField.TextField label={"Type"} />}
              </form.AppField>
              <Typography>{account.currency}</Typography>
              <Typography>{account.balance}</Typography>
              {typeof account.availableBalance !== "undefined" && (
                <Typography>{account.availableBalance}</Typography>
              )}
            </Paper>
          ))}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={cancelImport}>Cancel</Button>
        <Button
          variant="contained"
          disabled={accounts.length <= 0}
          onClick={onImportBtnClick}
        >
          Import
        </Button>
      </DialogActions>
    </>
  )
}
