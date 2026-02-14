import { Divider, Paper, Stack, Typography } from "@mui/material"
import type { FC } from "react"

/**
 * MenuRibbon - Top navigation bar that spans the full width of the application
 * Currently uses the DesktopMenuBar as the implementation
 *
 * TODO: This is a PLACEHOLDER - Replace with actual ribbon menu implementation
 */
export const MenuRibbon: FC = () => {
  return (
    <Paper
      elevation={1}
      sx={{
        width: "100%",
        flexShrink: 0,
        zIndex: (theme) => theme.zIndex.appBar,
        border: "none",
        borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
        borderRadius: 0,
        position: "relative",
        padding: 2,
      }}
    >
      <Stack
        direction={"row"}
        sx={{
          gap: 2,
          textAlign: "center",
          fontStyle: "italic",
          color: (theme) => theme.palette.text.disabled,
        }}
      >
        <Typography>MenuRibbon</Typography>
        <Divider orientation={"vertical"} flexItem />
        <Typography>Waiting for implementation...</Typography>
      </Stack>
    </Paper>
  )
}
