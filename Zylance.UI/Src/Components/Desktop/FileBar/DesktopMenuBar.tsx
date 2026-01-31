import { FileMenu } from "@/Components/Desktop/FileBar/Menus/FileMenu.tsx"
import { HelpMenu } from "@/Components/Desktop/FileBar/Menus/HelpMenu.tsx"
import { Stack } from "@mui/material"
import { type FC, useState } from "react"

import styles from "./FileBar.module.css"

export const DesktopMenuBar: FC = () => {
  const [activeMenu, setActiveMenu] = useState<string | null>(null)

  const menus = {
    file: FileMenu,
    help: HelpMenu,
  }

  return (
    <Stack direction="row" bgcolor="grey.200" className={styles.menuBar}>
      {Object.entries(menus).map(([key, MenuComponent]) => (
        <MenuComponent
          key={key}
          open={activeMenu === key}
          onClick={() => setActiveMenu(key)}
          onClose={() => setActiveMenu(null)}
        />
      ))}
    </Stack>
  )
}
