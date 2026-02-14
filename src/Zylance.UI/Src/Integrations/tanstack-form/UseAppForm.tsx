import { createFormHook } from "@tanstack/react-form"
import { FilePickerField } from "@/Integrations/tanstack-form/Fields/FilePickerField"
import { formContext, fieldContext } from "@/Integrations/tanstack-form/AppFormContext"

export const { useAppForm } = createFormHook({
  fieldContext,
  formContext,
  fieldComponents: { FilePickerField },
  formComponents: {},
})
