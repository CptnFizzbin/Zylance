import { Button, Grow, MenuList, Paper, Popper } from '@mui/material';
import {
  type FC,
  type KeyboardEvent,
  type PropsWithChildren,
  useEffect,
  useRef,
} from 'react';

export interface MenuBarMenuProps extends PropsWithChildren {
  id: string;
  label: string;
  open: boolean;
  onClick: () => void;
  onClose: () => void;
}

export const FileBarMenu: FC<MenuBarMenuProps> = ({
  id,
  label,
  open,
  onClick,
  onClose,
  children,
}) => {
  const anchorRef = useRef<HTMLButtonElement>(null);

  function handleListKeyDown (event: KeyboardEvent) {
    if (event.key === 'Tab') {
      event.preventDefault();
    } else if (event.key === 'Escape') {
      onClose();
    }
  }

  // return focus to the button when we transitioned from !open -> open
  const prevOpen = useRef(open);
  useEffect(() => {
    if (prevOpen.current && !open) {
      anchorRef.current?.focus();
    }

    prevOpen.current = open;
  }, [open]);

  return (
    <>
      <Button
        ref={anchorRef}
        id={`menuButton-${id}`}
        aria-controls={open ? `${label}-menu` : undefined}
        aria-expanded={open ? 'true' : undefined}
        aria-haspopup="true"
        onClick={() => onClick()}
        sx={{
          fontSize: '12px',
          padding: '2px 4px',
          color: 'text.primary',
          minWidth: 0,
          textTransform: 'capitalize',
          borderRadius: 1,
          transition: 'all 0.2s ease-in-out',
          '&:hover': {
            backgroundColor: (theme) => `${theme.palette.primary.main}15`,
            color: 'primary.main',
          },
          ...(open && {
            backgroundColor: (theme) => `${theme.palette.primary.main}20`,
            color: 'primary.main',
          }),
        }}
      >
        {label}
      </Button>

      <Popper
        open={open}
        anchorEl={anchorRef.current}
        placement="bottom-start"
        transition
        disablePortal
      >
        {({ TransitionProps, placement }) => (
          <Grow
            {...TransitionProps}
            style={{
              transformOrigin:
                placement === 'bottom-start' ? 'left top' : 'left bottom',
            }}
          >
            <Paper
              sx={{
                width: 320,
                maxWidth: '100%',
              }}
            >
              <MenuList
                dense
                autoFocusItem={open}
                id={`menu-${id}`}
                aria-labelledby={`menuButton-${id}`}
                onKeyDown={handleListKeyDown}
              >
                {children}
              </MenuList>
            </Paper>
          </Grow>
        )}
      </Popper>
    </>
  );
};
