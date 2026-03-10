import Box from "@mui/material/Box"
import LinearProgress from "@mui/material/LinearProgress"
import Paper from "@mui/material/Paper"
import Typography from "@mui/material/Typography"
import type { FC } from "react"

export interface LoadingScreenProps {
  text?: string
  progress?: number // 0-100 determinate progress
}

export const LoadingScreen: FC<LoadingScreenProps> = ({ text, progress }) => {
  const determinate =
    typeof progress === "number" && progress >= 0 && progress <= 100

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        flexGrow: 1,
        padding: 2,
      }}
    >
      <Paper
        elevation={3}
        sx={{
          width: "100%",
          maxWidth: 720,
          paddingX: 3,
          paddingY: 4,
          display: "flex",
          flexDirection: "column",
          alignItems: "stretch",
        }}
      >
        <Box
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            alignItems: "stretch",
          }}
        >
          {text && (
            <Typography variant="body1" align="center">
              {text}
            </Typography>
          )}

          <Box sx={{ display: "flex", alignItems: "center" }}>
            <Box sx={{ flexGrow: 1 }}>
              <LinearProgress
                variant={determinate ? "determinate" : "indeterminate"}
                value={determinate ? progress : undefined}
              />
            </Box>
          </Box>
        </Box>
      </Paper>
    </Box>
  )
}
