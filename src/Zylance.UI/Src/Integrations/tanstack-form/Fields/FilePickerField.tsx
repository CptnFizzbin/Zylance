import type { FileFilter, FileRef } from "$Contract/models/File"
import { Button, Stack, TextField } from "@mui/material"
import type { FC } from "react"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { useFieldContext } from "../AppFormContext"

interface SelectFileProps {
  mode: "select"
  readonly?: boolean
}

interface CreateFileProps {
  mode: "create"
  filename?: string
}

type FilePickerFieldProps = (SelectFileProps | CreateFileProps) & {
  showFilename?: boolean
  label: string
  title?: string
  filters: FileFilter[]
}

export const FilePickerField: FC<FilePickerFieldProps> = ({
  label,
  filters,
  ...props
}) => {
  const field = useFieldContext<FileRef>()
  const zylanceApi = useZylanceApi()

  const handleButtonClick = async () => {
    try {
      const fileRef =
        props.mode === "create"
          ? (
            await zylanceApi.files.createFile({
              filters,
              filename: props.filename,
            })
          ).fileRef
          : (
            await zylanceApi.files.selectFile({
              filters,
              readOnly: props.readonly ?? true,
            })
          ).fileRef
      if (fileRef) {
        field.handleChange(fileRef)
      }
    } catch (err) {
      // TODO: Handle error (e.g. show toast notification)
      console.error(err)
    }
  }

  return (
    <Stack direction="row" alignItems="center">
      <Button
        variant="outlined"
        onClick={handleButtonClick}
        sx={{ borderTopRightRadius: 0, borderBottomRightRadius: 0 }}
      >
        Select File
      </Button>
      <TextField
        value={field.state.value?.filename || ""}
        size="small"
        variant="outlined"
        placeholder={"No file selected"}
        disabled
        slotProps={{
          input: {
            sx: {
              borderTopLeftRadius: 0,
              borderBottomLeftRadius: 0,
              flex: 1,
              borderLeft: "none",
            },
          },
        }}
      />
    </Stack>
  )
}
