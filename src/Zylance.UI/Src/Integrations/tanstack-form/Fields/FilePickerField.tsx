import type { FC } from "react"
import { Button, Box, TextField } from "@mui/material"
import { useFieldContext } from "../AppFormContext"
import type { FileRef, FileFilter } from "@Contract/models/File"
import { useZylanceApi } from "@/Hooks/UseZylance"

interface SelectFileProps {
  mode: "select"
  readonly?: boolean
}

interface CreateFileProps {
  mode: "create"
  filename?: string
}

type FilePickerFieldProps = (SelectFileProps | CreateFileProps) & {
  label: string
  title?: string
  filters: FileFilter[],
}

export const FilePickerField: FC<FilePickerFieldProps> = ({ label, filters, ...props }) => {
  const field = useFieldContext<FileRef>()
  const zylanceApi = useZylanceApi()

  const handleButtonClick = async () => {
    try {
      const fileRef = props.mode === "create"
        ? (await zylanceApi.files.createFile({ filters, filename: props.filename })).fileRef
        : (await zylanceApi.files.selectFile({ filters, readOnly: props.readonly ?? true })).fileRef
      if (fileRef) {
        field.handleChange(fileRef)
      }
    } catch (err) {
      // Optionally handle error (show toast, etc)
    }
  }

  return (
    <Box display="flex" alignItems="center" gap={1}>
      <Button variant="outlined" onClick={handleButtonClick}>
        {label}
      </Button>
      <TextField
        value={field.state.value?.filename || ""}
        size="small"
        variant="outlined"
        slotProps={{ input: { readOnly: true } }}
        sx={{ flex: 1 }}
      />
    </Box>
  )
}
