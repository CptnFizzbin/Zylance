import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Divider,
  Stack,
  Typography,
} from "@mui/material"
import { useMutation } from "@tanstack/react-query"
import { createFileRoute } from "@tanstack/react-router"
import { useZylance } from "@/Hooks/UseZylance"

export const Route = createFileRoute("/locked/select-vault")({
  component: RouteComponent,
})

function RouteComponent() {
  console.log("Rendering /_locked/vault/select route")

  const navigate = Route.useNavigate()
  const { zylanceApi } = useZylance()

  const openVaultMutation = useMutation({
    mutationFn: async () => zylanceApi.vault.openVault(),
    onSuccess: (data) => {
      if (data.vaultRef) {
        console.log("Vault opened successfully:", data.vaultRef)
        navigate({ to: "/locked/unlock-vault" })
      } else {
        openVaultMutation.reset()
        // Set error via the mutation's error state instead
        throw new Error("No vault selected")
      }
    },
    onError: (error) => {
      console.error("Error opening vault:", error)
    },
  })

  const createVaultMutation = useMutation({
    mutationFn: async () => zylanceApi.vault.createVault(),
    onSuccess: (data) => {
      if (data.vaultRef) {
        console.log("Vault created successfully:", data.vaultRef)
        navigate({ to: "/locked/unlock-vault" })
      } else {
        createVaultMutation.reset()
        throw new Error("Failed to create vault")
      }
    },
    onError: (error) => {
      console.error("Error creating vault:", error)
    },
  })

  const isLoading = openVaultMutation.isPending || createVaultMutation.isPending
  const error = openVaultMutation.error || createVaultMutation.error

  return (
    <Box
      sx={{
        height: "100%",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background:
          "linear-gradient(135deg, #0a0a0a 0%, #1a1a1a 50%, #0a0a0a 100%)",
      }}
    >
      <Container maxWidth="sm">
        <Stack spacing={4} sx={{ alignItems: "center" }}>
          {/* Logo and Title Section */}
          <Stack spacing={2} sx={{ alignItems: "center", textAlign: "center" }}>
            <Typography
              variant="h2"
              component="h1"
              sx={{
                fontWeight: "bold",
                background: "linear-gradient(135deg, #d4af37 0%, #c0c0c0 100%)",
                backgroundClip: "text",
                WebkitBackgroundClip: "text",
                WebkitTextFillColor: "transparent",
              }}
            >
              Zylance
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Your Personal Finance Vault
            </Typography>
          </Stack>

          <Card sx={{ width: "100%" }}>
            <CardContent sx={{ p: 4 }}>
              <Typography
                variant="h5"
                component="h2"
                gutterBottom
                role={"heading"}
                sx={{ fontWeight: "600", textAlign: "center", mb: 3 }}
              >
                Select Your Vault
              </Typography>

              {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                  {error.message || "An error occurred. Please try again."}
                </Alert>
              )}

              <Stack spacing={2}>
                <Button
                  variant="contained"
                  size="large"
                  fullWidth
                  onClick={() => openVaultMutation.mutate()}
                  disabled={isLoading}
                  sx={{ py: 1.5 }}
                >
                  {openVaultMutation.isPending
                    ? "Opening..."
                    : "Open Existing Vault"}
                </Button>

                <Divider sx={{ my: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    or
                  </Typography>
                </Divider>

                <Button
                  variant="contained"
                  size="large"
                  fullWidth
                  onClick={() => createVaultMutation.mutate()}
                  disabled={isLoading}
                  color="success"
                  sx={{ py: 1.5 }}
                >
                  {createVaultMutation.isPending
                    ? "Creating..."
                    : "Create New Vault"}
                </Button>
              </Stack>

              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ textAlign: "center", mt: 3 }}
              >
                All data is stored locally.
              </Typography>
            </CardContent>
          </Card>

          <Typography
            variant="caption"
            color="text.disabled"
            sx={{ textAlign: "center" }}
          >
            © 2026 CptnFizzbin
          </Typography>
        </Stack>
      </Container>
    </Box>
  )
}
