import type { FC } from "react"
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  type DialogProps,
  DialogTitle,
  Link,
  Typography,
} from "@mui/material"

export interface AboutDialogProps extends DialogProps {
  onClose?: () => void
}

export const AboutDialog: FC<AboutDialogProps> = ({ onClose, ...props }) => {
  return (
    <Dialog {...props} onClose={onClose}>
      <DialogTitle>About Zylance</DialogTitle>
      <DialogContent>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          <Typography variant="h6" component="div">
            Zylance Finance Manager
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Version 0.1.0-alpha
          </Typography>
          <Typography variant="body1">
            An open-source finance and budgeting application built with clean
            architecture.
          </Typography>
          <Box>
            <Typography variant="body2" color="text.secondary">
              Built with:
            </Typography>
            <Typography variant="body2" sx={{ ml: 2 }}>
              • React + TypeScript
            </Typography>
            <Typography variant="body2" sx={{ ml: 2 }}>
              • .NET 10.0
            </Typography>
            <Typography variant="body2" sx={{ ml: 2 }}>
              • Photino.NET
            </Typography>
          </Box>
          <Typography variant="body2">
            <Link
              href="https://github.com/cptnfizzbin/Zylance"
              target="_blank"
              rel="noopener"
            >
              View on GitHub
            </Link>
          </Typography>
          <Typography variant="caption" color="text.secondary">
            © 2026 Zylance Project. Licensed under MIT License.
          </Typography>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  )
}
