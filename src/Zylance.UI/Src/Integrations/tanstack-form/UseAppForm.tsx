import { createFormHook } from "@tanstack/react-form"
import { fieldContext, formContext } from "./AppFormContext"
import { FilePickerField } from "./Fields/FilePickerField"
import { SelectField } from "./Fields/SelectField"
import { TextField } from "./Fields/TextField"
import { CheckboxField } from "./Fields/CheckboxField"
import { CurrencyField } from "./Fields/CurrencyField"

export const { useAppForm } = createFormHook({
  fieldContext,
  formContext,
  fieldComponents: {
    FilePickerField,
    SelectField,
    CheckboxField,
    TextField,
    CurrencyField,
  },
  formComponents: {},
})
