import AddIcon from "@mui/icons-material/Add"
import DeleteIcon from "@mui/icons-material/Delete"
import SearchIcon from "@mui/icons-material/Search"
import {
  Box,
  Button,
  Divider,
  IconButton,
  InputAdornment,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Paper,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material"
import type { FC } from "react"
import { useCallback, useMemo, useState } from "react"
import { v4 as uuidv4 } from "uuid"

interface Note {
  id: string
  title: string
  content: string
  createdAt: Date
  updatedAt: Date
}

export const NotesPanel: FC = () => {
  const [notes, setNotes] = useState<Note[]>([])
  const [selectedNoteId, setSelectedNoteId] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState("")

  const filteredNotes = useMemo(() => {
    if (!searchQuery.trim()) return notes
    const query = searchQuery.toLowerCase()
    return notes.filter(
      (note) =>
        note.title.toLowerCase().includes(query) ||
        note.content.toLowerCase().includes(query),
    )
  }, [notes, searchQuery])

  const selectedNote = notes.find((note) => note.id === selectedNoteId) ?? null

  const addNote = useCallback(() => {
    const newNote: Note = {
      id: uuidv4(),
      title: "",
      content: "",
      createdAt: new Date(),
      updatedAt: new Date(),
    }
    setNotes((prev) => [newNote, ...prev])
    setSelectedNoteId(newNote.id)
  }, [])

  const updateNote = useCallback(
    (id: string, updates: Partial<Pick<Note, "title" | "content">>) => {
      setNotes((prev) =>
        prev.map((note) =>
          note.id === id
            ? { ...note, ...updates, updatedAt: new Date() }
            : note,
        ),
      )
    },
    [],
  )

  const deleteNote = useCallback((id: string) => {
    setNotes((prev) => prev.filter((note) => note.id !== id))
    setSelectedNoteId((prev) => (prev === id ? null : prev))
  }, [])

  return (
    <Box sx={{ display: "flex", height: "100%" }}>
      {/* Left panel: search + notes list */}
      <Paper
        elevation={0}
        sx={{
          width: 280,
          flexShrink: 0,
          display: "flex",
          flexDirection: "column",
          borderRight: (theme) => `1px solid ${theme.palette.divider}`,
          borderRadius: 0,
        }}
      >
        {/* Header */}
        <Box
          sx={{
            p: 2,
            borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
          }}
        >
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 1.5,
            }}
          >
            <Typography variant="h6" component="h2">
              Notes
            </Typography>
            <Button
              size="small"
              startIcon={<AddIcon />}
              onClick={addNote}
              variant="contained"
            >
              New
            </Button>
          </Box>
          <TextField
            size="small"
            placeholder="Search notes…"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            fullWidth
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
          />
        </Box>

        {/* Notes list */}
        <List sx={{ flexGrow: 1, overflow: "auto", p: 0 }}>
          {filteredNotes.length === 0 ? (
            <Box
              sx={{
                p: 2,
                textAlign: "center",
                color: "text.disabled",
                fontStyle: "italic",
              }}
            >
              <Typography variant="body2">
                {notes.length === 0
                  ? "No notes yet"
                  : "No notes match your search"}
              </Typography>
            </Box>
          ) : (
            filteredNotes.map((note) => (
              <ListItem key={note.id} disablePadding>
                <ListItemButton
                  selected={note.id === selectedNoteId}
                  onClick={() => setSelectedNoteId(note.id)}
                >
                  <ListItemText
                    primary={note.title || "Untitled"}
                    secondary={
                      note.content.slice(0, 60) +
                      (note.content.length > 60 ? "…" : "")
                    }
                    primaryTypographyProps={{ noWrap: true }}
                    secondaryTypographyProps={{ noWrap: true }}
                  />
                </ListItemButton>
              </ListItem>
            ))
          )}
        </List>
      </Paper>

      {/* Right panel: note editor */}
      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          overflow: "hidden",
        }}
      >
        {selectedNote ? (
          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              height: "100%",
              p: 2,
              gap: 1.5,
            }}
          >
            {/* Title row */}
            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
              <TextField
                value={selectedNote.title}
                onChange={(e) =>
                  updateNote(selectedNote.id, { title: e.target.value })
                }
                placeholder="Note title"
                variant="standard"
                fullWidth
                slotProps={{
                  input: {
                    style: { fontSize: "1.25rem", fontWeight: 500 },
                  },
                }}
              />
              <Tooltip title="Delete note">
                <IconButton
                  onClick={() => deleteNote(selectedNote.id)}
                  size="small"
                  color="error"
                >
                  <DeleteIcon />
                </IconButton>
              </Tooltip>
            </Box>

            <Divider />

            {/* Monospaced content area */}
            <TextField
              value={selectedNote.content}
              onChange={(e) =>
                updateNote(selectedNote.id, { content: e.target.value })
              }
              placeholder="Note content…"
              multiline
              fullWidth
              variant="outlined"
              sx={{
                flexGrow: 1,
                "& .MuiInputBase-root": {
                  fontFamily: "monospace",
                  height: "100%",
                  alignItems: "flex-start",
                },
                "& .MuiInputBase-input": {
                  height: "100% !important",
                  overflow: "auto !important",
                },
              }}
            />
          </Box>
        ) : (
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              height: "100%",
              color: "text.disabled",
            }}
          >
            <Box sx={{ textAlign: "center" }}>
              <Typography variant="h6" fontStyle="italic">
                Select a note or create a new one
              </Typography>
              <Button
                sx={{ mt: 2 }}
                startIcon={<AddIcon />}
                onClick={addNote}
                variant="outlined"
              >
                New Note
              </Button>
            </Box>
          </Box>
        )}
      </Box>
    </Box>
  )
}
