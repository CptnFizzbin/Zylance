import { Checkbox, type CheckboxProps, FormControlLabel, FormHelperText } from "@mui/material"
import type { FC } from "react"
import { useFieldContext } from "../AppFormContext"

export interface CheckboxFieldProps
  extends Omit<CheckboxProps, "checked" | "onChange"> {
  label: string
  name: string
  disabled?: boolean
  error?: string
}

export const CheckboxField: FC<CheckboxFieldProps> = ({
  label,
  name,
  disabled,
  error,
}) => {
  const field = useFieldContext<boolean>()

  return (
    <>
      <FormControlLabel
        control={
          <Checkbox
            checked={field.state.value}
            onChange={(e) => field.handleChange(e.target.checked)}
            name={name}
            disabled={disabled}
            aria-label={label}
          />
        }
        label={label}
      />
      {error && <FormHelperText error>{error}</FormHelperText>}
    </>
  )
}
