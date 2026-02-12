import { ClickAwayListener, Stack } from '@mui/material';
import { type FC, type SyntheticEvent, useRef, useState } from 'react';
import { FileMenu } from '@/Components/Desktop/FileBar/Menus/FileMenu.tsx';
import { HelpMenu } from '@/Components/Desktop/FileBar/Menus/HelpMenu.tsx';

export const DesktopMenuBar: FC = () => {
  const anchorRef = useRef<HTMLDivElement>(null);
  const [activeMenu, setActiveMenu] = useState<string | null>(null);

  const menus = {
    file: FileMenu,
    help: HelpMenu,
  };

  const onClickAway = (event: Event | SyntheticEvent) => {
    if (anchorRef.current?.contains(event.target as HTMLElement)) {
      return;
    }

    setActiveMenu(null);
  };

  const onMenuClick = (menuKey: string) => {
    setActiveMenu((prevMenu) => (prevMenu === menuKey ? null : menuKey));
  };

  return (
    <ClickAwayListener onClickAway={onClickAway}>
      <Stack
        direction="row"
        ref={anchorRef}
        id="menu-bar"
        sx={{
          zIndex: 10000,
          padding: 0.5,
          gap: 0.5,
          borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
        }}
      >
        {Object.entries(menus).map(([key, MenuComponent]) => (
          <MenuComponent
            key={key}
            id={key}
            open={activeMenu === key}
            onClick={() => onMenuClick(key)}
            onClose={() => setActiveMenu(null)}
          />
        ))}
      </Stack>
    </ClickAwayListener>
  );
};
