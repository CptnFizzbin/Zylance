import LinearProgress, {
  type LinearProgressProps,
} from "@mui/material/LinearProgress"
import type { FC } from "react"

export const ProgressBar: FC<LinearProgressProps> = (props) => {
  return <LinearProgress variant="indeterminate" {...props} />
}
