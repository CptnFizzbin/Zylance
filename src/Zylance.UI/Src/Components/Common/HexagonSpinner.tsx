import Box from "@mui/material/Box"
import { keyframes, styled } from "@mui/material/styles"
import type { FC } from "react"

const spin = keyframes`
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
`

const Hex = styled(Box, {
  shouldForwardProp: (prop) => prop !== "size" && prop !== "color",
})<{
  size?: number
  color?: string
}>(({ size = 40, color = "#1976d2" }) => ({
  width: size,
  height: size,
  display: "inline-block",
  clipPath:
    "polygon(25% 6.7%, 75% 6.7%, 100% 50%, 75% 93.3%, 25% 93.3%, 0% 50%)",
  backgroundColor: color,
  animation: `${spin} 1s linear infinite`,
}))

export interface HexagonSpinnerProps {
  size?: number
  color?: string
  "aria-label"?: string
}

export const HexagonSpinner: FC<HexagonSpinnerProps> = ({
  size = 40,
  color = "#1976d2",
  "aria-label": ariaLabel = "Loading",
}) => {
  return <Hex size={size} color={color} role="status" aria-label={ariaLabel} />
}
