import { createFormHook } from "@tanstack/react-form"
import { fieldContext, formContext } from "@/Integrations/tanstack-form/AppFormContext"
import { FilePickerField } from "@/Integrations/tanstack-form/Fields/FilePickerField"
import { SelectField } from "@/Integrations/tanstack-form/Fields/SelectField"
import { TextField } from "@/Integrations/tanstack-form/Fields/TextField"
import { CheckboxField } from "./Fields/CheckboxField"

export const { useAppForm } = createFormHook({
  fieldContext,
  formContext,
  fieldComponents: {
    FilePickerField,
    SelectField,
    CheckboxField,
    TextField,
  },
  formComponents: {},
})
