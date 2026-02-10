import ExpandMoreIcon from "@mui/icons-material/ExpandMore"
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  LinearProgress,
  List,
  ListItem,
  Stack,
  Typography,
} from "@mui/material"
import type { FC } from "react"
import { useCallback, useEffect, useRef, useState } from "react"

export interface ImportDialogProps {
  open: boolean
  onClose?: () => void
  onCancel?: () => void
}

type Step = "select" | "accounts" | "process" | "results"

export const ImportDialog: FC<ImportDialogProps> = ({
  open,
  onClose,
  onCancel,
}) => {
  const [expanded, setExpanded] = useState<Step | false>("select")
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [accounts, setAccounts] = useState(
    () =>
      [
        { id: "acct-1", name: "Checking Account", checked: true },
        { id: "acct-2", name: "Savings Account", checked: false },
        { id: "acct-3", name: "Credit Card", checked: false },
      ] as { id: string; name: string; checked: boolean }[],
  )
  const [isImporting, setIsImporting] = useState(false)
  const [progress, setProgress] = useState(0)
  const [results, setResults] = useState<string | null>(null)
  const progressRef = useRef<number>(0)
  const timerRef = useRef<number | null>(null)

  // Helper to move to a step and expand that accordion
  const goToStep = useCallback((step: Step) => void setExpanded(step), [])

  const cleanupTimer = useCallback(() => {
    if (timerRef.current != null) {
      window.clearInterval(timerRef.current)
      timerRef.current = null
    }
  }, [])

  const startSimulatedImport = useCallback(() => {
    cleanupTimer()
    setProgress(0)
    progressRef.current = 0

    timerRef.current = window.setInterval(() => {
      // Increment progress by a small random amount to feel realistic
      const inc = Math.floor(Math.random() * 8) + 3 // 3..10
      progressRef.current = Math.min(100, progressRef.current + inc)
      setProgress(progressRef.current)

      // Auto-open the results when complete
      if (progressRef.current >= 100) {
        cleanupTimer()
        setIsImporting(false)
        setResults("Imported successfully to selected accounts.")
        goToStep("results")
      }
    }, 400)
    // cleanupTimer and goToStep are stable (useCallback with empty deps)
  }, [cleanupTimer, goToStep])

  useEffect(() => {
    // Reset state whenever dialog is opened/closed
    if (!open) {
      cleanupTimer()
      setIsImporting(false)
      setProgress(0)
      progressRef.current = 0
      setResults(null)
      setSelectedFile(null)
      setExpanded("select")
    }
  }, [open, cleanupTimer])

  useEffect(() => {
    // If importing starts, ensure the import accordion is visible
    if (isImporting) {
      goToStep("process")
      startSimulatedImport()
    }
  }, [isImporting, goToStep, startSimulatedImport])

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const f = e.target.files?.[0] ?? null
    setSelectedFile(f)
  }

  function toggleAccount(id: string) {
    setAccounts((prev) =>
      prev.map((a) => (a.id === id ? { ...a, checked: !a.checked } : a)),
    )
  }

  function handleStartImport() {
    // Basic validation: file and at least one account
    if (!selectedFile) {
      setExpanded("select")
      return
    }
    if (!accounts.some((a) => a.checked)) {
      setExpanded("accounts")
      return
    }

    setIsImporting(true)
    setResults(null)
  }

  function handleCancel() {
    // Cancel any running import
    cleanupTimer()
    setIsImporting(false)
    setProgress(0)
    progressRef.current = 0
    setResults("Import cancelled by user.")
    goToStep("results")

    if (onCancel) onCancel()
  }

  function handleClose() {
    cleanupTimer()
    if (onClose) onClose()
  }

  return (
    <Dialog
      fullWidth
      maxWidth="md"
      open={open}
      onClose={handleClose}
      aria-labelledby="import-dialog-title"
    >
      <DialogTitle id="import-dialog-title">Import</DialogTitle>
      <DialogContent dividers>
        <Accordion expanded={expanded === "select"}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="subtitle1">Select File</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={1}>
              <input
                id="file-input"
                type="file"
                accept=".qfx,.ofx,.csv,.json"
                onChange={handleFileChange}
                aria-label="Select import file"
              />
              <Typography variant="body2" color="text.secondary">
                {selectedFile
                  ? `Selected: ${selectedFile.name}`
                  : "No file selected."}
              </Typography>
              <Stack direction="row" spacing={1}>
                <Button
                  variant="contained"
                  onClick={() => goToStep("accounts")}
                  disabled={!selectedFile}
                >
                  Next: Accounts
                </Button>
                <Button
                  onClick={() => {
                    setSelectedFile(null)
                  }}
                >
                  Clear
                </Button>
              </Stack>
            </Stack>
          </AccordionDetails>
        </Accordion>

        <Accordion expanded={expanded === "accounts"}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="subtitle1">Accounts</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={1}>
              <Typography variant="body2">
                Select which accounts to import into.
              </Typography>
              <List>
                {accounts.map((acct) => (
                  <ListItem key={acct.id} disableGutters>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={acct.checked}
                          onChange={() => toggleAccount(acct.id)}
                        />
                      }
                      label={acct.name}
                    />
                  </ListItem>
                ))}
              </List>
              <Stack direction="row" spacing={1}>
                <Button variant="contained" onClick={() => goToStep("process")}>
                  Start Import
                </Button>
                <Button onClick={() => goToStep("select")}>Back</Button>
              </Stack>
            </Stack>
          </AccordionDetails>
        </Accordion>

        <Accordion expanded={expanded === "process"}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="subtitle1">Import Process</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={2}>
              <Typography variant="body2">
                The import will run and show progress here. This section opens
                automatically while importing.
              </Typography>

              <div>
                <LinearProgress variant="determinate" value={progress} />
                <Typography variant="caption">{progress}%</Typography>
              </div>

              <Stack direction="row" spacing={1}>
                <Button
                  variant="contained"
                  disabled={!selectedFile || isImporting}
                  onClick={handleStartImport}
                >
                  {isImporting ? "Importing..." : "Start Import"}
                </Button>
                <Button
                  onClick={() => goToStep("accounts")}
                  disabled={isImporting}
                >
                  Back
                </Button>
              </Stack>
            </Stack>
          </AccordionDetails>
        </Accordion>

        <Accordion expanded={expanded === "results"}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant="subtitle1">Results</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={1}>
              <Typography variant="body2">
                {results ?? "No results yet."}
              </Typography>
              <Stack direction="row" spacing={1}>
                <Button
                  onClick={() => {
                    setResults(null)
                    setProgress(0)
                    goToStep("select")
                  }}
                >
                  Import Another
                </Button>
                <Button onClick={handleClose}>Close</Button>
              </Stack>
            </Stack>
          </AccordionDetails>
        </Accordion>
      </DialogContent>

      <DialogActions>
        <Button color="error" onClick={handleCancel}>
          Cancel
        </Button>
        <Button onClick={handleClose}>Done</Button>
      </DialogActions>
    </Dialog>
  )
}
