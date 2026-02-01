import { alpha, createTheme } from "@mui/material"

/**
 * Glassmorphic Theme for Zylance
 *
 * This theme implements a modern glassmorphism design with elegant gold and silver/gray colors.
 * Features:
 * - Translucent backgrounds with backdrop blur effects
 * - Gold accent colors for a premium, sophisticated aesthetic
 * - Light mode: Gold and silver palette
 * - Dark mode: Gold and gray palette
 * - Elevated components with subtle shadows and borders
 */
export const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#d4af37", // Rich gold
      light: "#f4d03f",
      dark: "#b8941e",
      contrastText: "#1a1a1a",
    },
    secondary: {
      main: "#8c8c8c", // Elegant gray (dark mode)
      light: "#a8a8a8",
      dark: "#5a5a5a",
      contrastText: "#ffffff",
    },
    background: {
      default: "#1a1a1a", // Deep charcoal
      paper: alpha("#2a2a2a", 0.7), // Translucent dark gray
    },
    error: {
      main: "#d32f2f",
    },
    warning: {
      main: "#f57c00",
    },
    info: {
      main: "#b8941e",
    },
    success: {
      main: "#388e3c",
    },
    text: {
      primary: "#e8e8e8",
      secondary: alpha("#ffffff", 0.7),
      disabled: alpha("#ffffff", 0.5),
    },
    divider: alpha("#d4af37", 0.12),
  },
  typography: {
    fontFamily: [
      "-apple-system",
      "BlinkMacSystemFont",
      "\"Segoe UI\"",
      "Roboto",
      "\"Helvetica Neue\"",
      "Arial",
      "sans-serif",
    ].join(","),
    h1: {
      fontWeight: 700,
      letterSpacing: "-0.02em",
    },
    h2: {
      fontWeight: 700,
      letterSpacing: "-0.01em",
    },
    h3: {
      fontWeight: 600,
    },
    button: {
      fontWeight: 600,
      textTransform: "none", // More modern look
    },
  },
  shape: {
    borderRadius: 12, // Softer, more modern corners
  },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
          backdropFilter: "blur(20px)",
          border: `1px solid ${alpha("#d4af37", 0.1)}`,
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
          backgroundColor: alpha("#1a1a1a", 0.6),
          backdropFilter: "blur(20px)",
          border: `1px solid ${alpha("#d4af37", 0.15)}`,
          transition: "all 0.3s ease-in-out",
          "&:hover": {
            border: `1px solid ${alpha("#d4af37", 0.3)}`,
            boxShadow: `0 8px 32px ${alpha("#d4af37", 0.15)}`,
            transform: "translateY(-2px)",
          },
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          backdropFilter: "blur(10px)",
          transition: "all 0.2s ease-in-out",
        },
        contained: {
          boxShadow: `0 4px 12px ${alpha("#d4af37", 0.3)}`,
          "&:hover": {
            boxShadow: `0 6px 20px ${alpha("#d4af37", 0.4)}`,
            transform: "translateY(-1px)",
          },
        },
        outlined: {
          borderWidth: "2px",
          "&:hover": {
            borderWidth: "2px",
            backgroundColor: alpha("#d4af37", 0.08),
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
          backgroundColor: alpha("#0a0a0a", 0.8),
          backdropFilter: "blur(20px)",
          borderBottom: `1px solid ${alpha("#d4af37", 0.1)}`,
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          backgroundImage: "none",
          backgroundColor: alpha("#1a1a1a", 0.95),
          backdropFilter: "blur(30px)",
          border: `1px solid ${alpha("#d4af37", 0.2)}`,
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          "& .MuiOutlinedInput-root": {
            backgroundColor: alpha("#1a1a1a", 0.5),
            backdropFilter: "blur(10px)",
            "&:hover": {
              backgroundColor: alpha("#1a1a1a", 0.7),
            },
            "&.Mui-focused": {
              backgroundColor: alpha("#1a1a1a", 0.8),
            },
          },
        },
      },
    },
    MuiBackdrop: {
      styleOverrides: {
        root: {
          backdropFilter: "blur(4px)",
          backgroundColor: alpha("#0a0a0a", 0.7),
        },
      },
    },
    MuiMenuItem: {
      styleOverrides: {
        root: {
          borderRadius: "6px",
          margin: "2px 8px",
          transition: "all 0.2s ease-in-out",
          "&:hover": {
            backgroundColor: alpha("#d4af37", 0.12),
          },
          "&.Mui-selected": {
            backgroundColor: alpha("#d4af37", 0.16),
            "&:hover": {
              backgroundColor: alpha("#d4af37", 0.2),
            },
          },
        },
      },
    },
    MuiListItemIcon: {
      styleOverrides: {
        root: {
          color: "inherit",
          minWidth: "36px",
        },
      },
    },
  },
  shadows: [
    "none",
    `0 2px 4px ${alpha("#000", 0.2)}`,
    `0 4px 8px ${alpha("#000", 0.2)}`,
    `0 6px 12px ${alpha("#000", 0.2)}`,
    `0 8px 16px ${alpha("#000", 0.2)}`,
    `0 10px 20px ${alpha("#000", 0.2)}`,
    `0 12px 24px ${alpha("#000", 0.2)}`,
    `0 14px 28px ${alpha("#000", 0.2)}`,
    `0 16px 32px ${alpha("#000", 0.2)}`,
    `0 18px 36px ${alpha("#000", 0.2)}`,
    `0 20px 40px ${alpha("#000", 0.2)}`,
    `0 22px 44px ${alpha("#000", 0.2)}`,
    `0 24px 48px ${alpha("#000", 0.2)}`,
    `0 26px 52px ${alpha("#000", 0.2)}`,
    `0 28px 56px ${alpha("#000", 0.2)}`,
    `0 30px 60px ${alpha("#000", 0.2)}`,
    `0 32px 64px ${alpha("#000", 0.2)}`,
    `0 34px 68px ${alpha("#000", 0.2)}`,
    `0 36px 72px ${alpha("#000", 0.2)}`,
    `0 38px 76px ${alpha("#000", 0.2)}`,
    `0 40px 80px ${alpha("#000", 0.2)}`,
    `0 42px 84px ${alpha("#000", 0.2)}`,
    `0 44px 88px ${alpha("#000", 0.2)}`,
    `0 46px 92px ${alpha("#000", 0.2)}`,
    `0 48px 96px ${alpha("#000", 0.2)}`,
  ],
})
