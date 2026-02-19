import type { SxProps } from "@mui/system"

export const mergeSxProps = (...props: (SxProps | undefined)[]): SxProps => {
  return props.flatMap((prop) => (Array.isArray(prop) ? prop : [prop]))
}
