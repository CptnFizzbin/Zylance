import AddIcon from "@mui/icons-material/Add"
import CloseIcon from "@mui/icons-material/Close"
import ExitToAppIcon from "@mui/icons-material/ExitToApp"
import FolderOpenIcon from "@mui/icons-material/FolderOpen"
import InputIcon from "@mui/icons-material/Input"
import { Divider, ListItemIcon, ListItemText, MenuItem, Typography } from "@mui/material"
import type { FC } from "react"
import { useZylanceApi } from "@/Apis/UseZylanceApi"
import { useZylance } from "@/Components/Application/UseZylance"
import { FileBarMenu, type MenuBarMenuProps } from "@/Components/Desktop/FileBar/Menus/MenuBase"
import { useImportService } from "@/Components/Import/ImportContext"

export const FileMenu: FC<Omit<MenuBarMenuProps, "label">> = ({
  onClose,
  ...props
}) => {
  const importService = useImportService()
  const zylanceApi = useZylanceApi()
  const { currentVault } = useZylance()

  const onMenuClick = (handler: () => void) => {
    return () => {
      handler()
      onClose()
    }
  }

  return (
    <FileBarMenu {...props} label={"File"} onClose={onClose}>
      <MenuItem onClick={onMenuClick(() => zylanceApi.vault.createVault())}>
        <ListItemIcon>
          <AddIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText>New Vault</ListItemText>
      </MenuItem>
      <MenuItem onClick={onMenuClick(() => zylanceApi.vault.openVault())}>
        <ListItemIcon>
          <FolderOpenIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText>Open Vault</ListItemText>
      </MenuItem>
      <MenuItem
        disabled={!currentVault}
        onClick={onMenuClick(() => zylanceApi.vault.closeVault())}
      >
        <ListItemIcon>
          <CloseIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText>Close Vault</ListItemText>
      </MenuItem>
      <Divider />
      <MenuItem
        disabled={!currentVault}
        onClick={onMenuClick(() => importService.openDialog())}
      >
        <ListItemIcon>
          <InputIcon fontSize="small" />
        </ListItemIcon>
        <ListItemText>Import Transactions</ListItemText>
      </MenuItem>
      <Divider />
      <MenuItem onClick={onMenuClick(() => zylanceApi.desktop.emitExit())}>
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
