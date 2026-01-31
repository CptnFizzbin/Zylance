import { Button, ClickAwayListener, Grow, MenuList, Paper, Popper } from "@mui/material"
import { type FC, type KeyboardEvent, type PropsWithChildren, type SyntheticEvent, useEffect, useRef } from "react"

import styles from "../FileBar.module.css"

export interface MenuBarMenuProps extends PropsWithChildren {
  label: string
  open: boolean
  onClick: () => void
  onClose: () => void
}

export const FileBarMenu: FC<MenuBarMenuProps> = ({
  label,
  open,
  onClick,
  onClose,
  children,
}) => {
  const anchorRef = useRef<HTMLButtonElement>(null)

  const onClickAway = (event: Event | SyntheticEvent) => {
    if (
      anchorRef.current &&
      anchorRef.current.contains(event.target as HTMLElement)
    ) {
      return
    }

    onClose()
  }

  function handleListKeyDown (event: KeyboardEvent) {
    if (event.key === "Tab") {
      event.preventDefault()
      onClickAway(event)
    } else if (event.key === "Escape") {
      onClose()
    }
  }

  // return focus to the button when we transitioned from !open -> open
  const prevOpen = useRef(open)
  useEffect(() => {
    if (prevOpen.current && !open) {
      anchorRef.current!.focus()
    }

    prevOpen.current = open
  }, [open])

  return (
    <>
      <Button
        ref={anchorRef}
        id={`${label}-menu-button`}
        aria-controls={open ? `${label}-menu` : undefined}
        aria-expanded={open ? "true" : undefined}
        aria-haspopup="true"
        onClick={() => onClick()}
        className={styles.menuButton}
      >
        {label}
      </Button>

      <Popper
        open={open}
        anchorEl={anchorRef.current}
        role={undefined}
        placement="bottom-start"
        transition
        disablePortal
      >
        {({ TransitionProps, placement }) => (
          <Grow
            {...TransitionProps}
            style={{
              transformOrigin:
                placement === "bottom-start" ? "left top" : "left bottom",
            }}
          >
            <Paper sx={{ width: 320, maxWidth: "100%" }}>
              <ClickAwayListener onClickAway={onClose}>
                <MenuList
                  dense
                  autoFocusItem={open}
                  id={`${label}-menu`}
                  aria-labelledby={`${label}-menu-button`}
                  onKeyDown={handleListKeyDown}
                >
                  {children}
                </MenuList>
              </ClickAwayListener>
            </Paper>
          </Grow>
        )}
      </Popper>
    </>
  )
}
