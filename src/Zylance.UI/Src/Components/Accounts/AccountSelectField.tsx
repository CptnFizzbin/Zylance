import type { FC } from "react"
import { useFieldContext } from "@/Integrations/tanstack-form/AppFormContext"
import { FormControl, InputLabel, Select, Alert, MenuItem, Stack, Typography } from "@mui/material"
import { useZylanceQueries } from "@/Apis/Zylance/ZylanceQueryKeys"
import { useQuery } from "@tanstack/react-query"
import LinearProgress from "@mui/material/LinearProgress"

export interface AccountSelectFieldProps {
  label: string
  placeholder?: string
  size?: "small" | "medium"
}

export const AccountSelectField: FC<AccountSelectFieldProps> = ({
  label,
  placeholder,
  size = "small",
}) => {
  const field = useFieldContext<string>()
  const zylanceQueries = useZylanceQueries()
  const accountsQuery = useQuery({
    ...zylanceQueries.accounts.list,
  })

  if (accountsQuery.isPending) return <LinearProgress />
  if (accountsQuery.isError) return <Alert severity={"error"}>Error loading accounts</Alert>
  const accounts = accountsQuery.data

  return (
    <FormControl fullWidth>
      <InputLabel>{label}</InputLabel>
      <Select
        value={field.state.value}
        label={label}
        onBlur={field.handleBlur}
        size={size}
        slotProps={{
          input: {
            placeholder,
          },
        }}
        onChange={(e) => field.handleChange(e.target.value)}
      >
        {accounts.map((account) => (
          <MenuItem key={account.id} value={account.id}>
            <Stack>
              {account.name}
              <Typography variant="caption">{account.type} - {account.id}</Typography>
            </Stack>
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  )
}
