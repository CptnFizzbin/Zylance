import {
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  type SelectProps,
} from "@mui/material"
import type { FC } from "react"
import { useFieldContext } from "../AppFormContext"

export interface SelectFieldOption {
  value: string
  label: string
}

export type SelectFieldProps = Omit<SelectProps, "value" | "onChange"> & {
  label: string
  placeholder?: string
  options: SelectFieldOption[]
}

export const SelectField: FC<SelectFieldProps> = ({
  label,
  placeholder,
  options,
  ...props
}) => {
  const field = useFieldContext<string>()
  const error = field.state.meta.errors.map((err) => err.message).join(", ")

  return (
    <FormControl fullWidth size="small" variant="outlined" error={!!error}>
      <InputLabel>{label}</InputLabel>
      <Select
        label={label}
        value={field.state.value ?? ""}
        onChange={(e) => {
          field.handleChange(e.target.value as string)
        }}
        displayEmpty
        {...props}
      >
        {placeholder && (
          <MenuItem value="" disabled>
            {placeholder}
          </MenuItem>
        )}
        {options.map((opt) => (
          <MenuItem key={opt.value} value={opt.value}>
            {opt.label}
          </MenuItem>
        ))}
      </Select>
      {error && <FormHelperText>{error}</FormHelperText>}
    </FormControl>
  )
}
