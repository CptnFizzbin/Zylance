import { FileBarMenu, type MenuBarMenuProps } from "@/Components/Desktop/FileBar/Menus/MenuBase"
import { useZylance } from "@Lib/ZylanceContext"
import ExitToAppIcon from "@mui/icons-material/ExitToApp"
import { ListItemIcon, ListItemText, MenuItem, Typography } from "@mui/material"
import type { FC } from "react"

export const FileMenu: FC<Omit<MenuBarMenuProps, "label">> = ({ onClose, ...props }) => {
  const zylance = useZylance()

  return (
    <FileBarMenu {...props} label={"File"} onClose={onClose}>
      <MenuItem
        onClick={() => {
          zylance.desktop.emitExit()
          onClose()
        }}
      >
        <ListItemIcon>
          <ExitToAppIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText>Exit</ListItemText>
        <Typography variant="body2" sx={{ color: "text.secondary" }}>
          Alt+F4
        </Typography>
      </MenuItem>
    </FileBarMenu>
  )
}
