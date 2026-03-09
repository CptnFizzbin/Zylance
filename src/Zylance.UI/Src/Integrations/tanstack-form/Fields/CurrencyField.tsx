import {
  TextField as MuiTextField,
  type TextFieldProps as MuiTextFieldProps,
  InputAdornment,
  CircularProgress,
} from "@mui/material"
import type { FC } from "react"
import { useFieldContext } from "../AppFormContext"
import getSymbolFromCurrency from "currency-symbol-map"
import { useAccountCurrency } from "@/Components/Accounts/UseAccountCurrency"

export type TextFieldProps = Omit<MuiTextFieldProps, "onChange" | "value"> & {
  accountId: string
}

export const CurrencyField: FC<TextFieldProps> = ({ name, label, accountId, ...props }) => {
  const field = useFieldContext<string>()
  const currencyQuery = useAccountCurrency({ accountId })
  const symbol = getSymbolFromCurrency(currencyQuery.currency || "") || "?"

  return (
    <MuiTextField
      label={label}
      name={name}
      value={field.state.value ?? ""}
      onChange={(e) => field.handleChange(e.target.value)}
      fullWidth
      size="small"
      variant="outlined"
      type="number"
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment
              position={"start"}
              sx={{
                paddingRight: 1,
                borderRight: "1px solid",
                borderColor: "divider",
              }}
            >
              {currencyQuery.isPending ? <CircularProgress size={16} /> : symbol}
            </InputAdornment>
          ),
        },
      }}
      {...props}
    />
  )
}
