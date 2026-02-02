import { ClickAwayListener, Stack } from "@mui/material"
import { type FC, type SyntheticEvent, useRef, useState } from "react"
import { FileMenu } from "@/Components/Desktop/FileBar/Menus/FileMenu.tsx"
import { HelpMenu } from "@/Components/Desktop/FileBar/Menus/HelpMenu.tsx"

import styles from "./FileBar.module.css"

export const DesktopMenuBar: FC = () => {
  const anchorRef = useRef<HTMLDivElement>(null)
  const [activeMenu, setActiveMenu] = useState<string | null>(null)

  const menus = {
    file: FileMenu,
    help: HelpMenu,
  }

  const onClickAway = (event: Event | SyntheticEvent) => {
    if (anchorRef.current?.contains(event.target as HTMLElement)) {
      return
    }

    setActiveMenu(null)
  }

  const onMenuClick = (menuKey: string) => {
    setActiveMenu((prevMenu) => (prevMenu === menuKey ? null : menuKey))
  }

  return (
    <ClickAwayListener onClickAway={onClickAway}>
      <Stack
        direction="row"
        className={styles.menuBar}
        ref={anchorRef}
        sx={{
          backgroundColor: (theme) =>
            theme.palette.mode === "dark"
              ? "rgba(10, 10, 10, 0.8)"
              : "rgba(255, 255, 255, 0.8)",
          backdropFilter: "blur(20px)",
          borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
          transition: "all 0.3s ease-in-out",
        }}
      >
        {Object.entries(menus).map(([key, MenuComponent]) => (
          <MenuComponent
            key={key}
            open={activeMenu === key}
            onClick={() => onMenuClick(key)}
            onClose={() => setActiveMenu(null)}
          />
        ))}
      </Stack>
    </ClickAwayListener>
  )
}
