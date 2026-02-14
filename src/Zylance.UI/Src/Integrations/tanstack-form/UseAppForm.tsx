import { createFormHook } from "@tanstack/react-form"
import {
  fieldContext,
  formContext,
} from "@/Integrations/tanstack-form/AppFormContext"
import { FilePickerField } from "@/Integrations/tanstack-form/Fields/FilePickerField"

export const { useAppForm } = createFormHook({
  fieldContext,
  formContext,
  fieldComponents: { FilePickerField },
  formComponents: {},
})
