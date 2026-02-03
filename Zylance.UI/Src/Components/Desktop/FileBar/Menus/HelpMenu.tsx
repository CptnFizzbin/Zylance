import InfoOutlineIcon from "@mui/icons-material/InfoOutline"
import { ListItemIcon, ListItemText, MenuItem } from "@mui/material"
import { type FC, useState } from "react"
import { AboutDialog } from "@/Components/About/AboutDialog.tsx"
import {
  FileBarMenu,
  type MenuBarMenuProps,
} from "@/Components/Desktop/FileBar/Menus/MenuBase.tsx"

export const HelpMenu: FC<Omit<MenuBarMenuProps, "label">> = ({
  onClose,
  ...props
}) => {
  const [aboutDialogOpen, setAboutDialogOpen] = useState(false)

  return (
    <>
      <FileBarMenu {...props} label={"Help"} onClose={onClose}>
        <MenuItem
          onClick={() => {
            setAboutDialogOpen(true)
            onClose()
          }}
        >
          <ListItemIcon>
            <InfoOutlineIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>About Zylance</ListItemText>
        </MenuItem>
      </FileBarMenu>

      <AboutDialog
        open={aboutDialogOpen}
        onClose={() => setAboutDialogOpen(false)}
      />
    </>
  )
}
