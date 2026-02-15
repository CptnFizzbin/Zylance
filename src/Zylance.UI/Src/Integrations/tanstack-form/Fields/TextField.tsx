import { TextField as MuiTextField, type TextFieldProps as MuiTextFieldProps } from "@mui/material"
import type { FC } from "react"
import { useFieldContext } from "../AppFormContext"

export type TextFieldProps = Omit<MuiTextFieldProps, "onChange" | "value"> & {}

export const TextField: FC<TextFieldProps> = ({ name, label, ...props }) => {
  const field = useFieldContext<string>()

  return (
    <MuiTextField
      label={label}
      name={name}
      value={field.state.value ?? ""}
      onChange={(e) => field.handleChange(e.target.value)}
      fullWidth
      size="small"
      variant="outlined"
      {...props}
    />
  )
}
