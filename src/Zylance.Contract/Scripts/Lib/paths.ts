import { fileURLToPath } from "node:url"
import { dirname, resolve, join } from "node:path"

export const SCRIPT_DIR = resolve(dirname(fileURLToPath(import.meta.url)), "..")
export const CONTRACT_DIR = resolve(SCRIPT_DIR, "..")
export const DEST_DIR = resolve(CONTRACT_DIR, "Generated")
export const HOME_DIR = process.env.HOME || process.env.USERPROFILE || ""
export const GRPC_TOOLS_BASE_DIR = join(HOME_DIR, ".nuget", "packages", "grpc.tools")
