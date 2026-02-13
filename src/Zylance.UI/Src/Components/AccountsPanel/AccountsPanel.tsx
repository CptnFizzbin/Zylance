import { Box, Divider, Paper, Typography } from "@mui/material"
import type { FC } from "react"

/**
 * AccountsPanel - Left sidebar displaying all user accounts
 *
 * TODO: This is a PLACEHOLDER - Implement with actual account data
 */
export const AccountsPanel: FC = () => {
  return (
    <Paper
      elevation={0}
      sx={{
        width: 280,
        flexShrink: 0,
        height: "100%",
        borderRight: (theme) => `1px solid ${theme.palette.divider}`,
        borderRadius: 0,
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      <Box
        sx={{
          p: 2,
          borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
          <Typography variant="h6" component="h2">
            Accounts
          </Typography>
        </Box>
        <Typography variant="caption" color="text.secondary">
          Account list will be displayed here
        </Typography>
      </Box>
      <Box
        sx={{
          flexGrow: 1,
          overflow: "auto",
          p: 2,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <Box
          sx={{
            textAlign: "center",
            fontStyle: "italic",
            color: (theme) => theme.palette.text.disabled,
          }}
        >
          <Typography>Accounts Panel</Typography>
          <Divider />
          <Typography>Waiting for implementation...</Typography>
        </Box>
      </Box>
    </Paper>
  )
}
