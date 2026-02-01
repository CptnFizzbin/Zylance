import { FileMenu } from "@/Components/Desktop/FileBar/Menus/FileMenu.tsx"
import { HelpMenu } from "@/Components/Desktop/FileBar/Menus/HelpMenu.tsx"
import { ClickAwayListener, Stack } from "@mui/material"
import { type FC, type SyntheticEvent, useRef, useState } from "react"

import styles from "./FileBar.module.css"

export const DesktopMenuBar: FC = () => {
  const anchorRef = useRef<HTMLDivElement>(null)
  const [activeMenu, setActiveMenu] = useState<string | null>(null)

  const menus = {
    file: FileMenu,
    help: HelpMenu,
  }

  const onClickAway = (event: Event | SyntheticEvent) => {
    if (
      anchorRef.current &&
      anchorRef.current.contains(event.target as HTMLElement)
    ) {
      return
    }

    setActiveMenu(null)
  }

  return (
    <ClickAwayListener onClickAway={onClickAway}>
      <Stack direction="row" bgcolor="grey.200" className={styles.menuBar} ref={anchorRef}>
        {Object.entries(menus).map(([key, MenuComponent]) => (
          <MenuComponent
            key={key}
            open={activeMenu === key}
            onClick={() => setActiveMenu(key)}
            onClose={() => setActiveMenu(null)}
          />
        ))}
      </Stack>
    </ClickAwayListener>
  )
}
