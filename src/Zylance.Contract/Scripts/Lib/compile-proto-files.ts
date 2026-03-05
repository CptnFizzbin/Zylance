import path, { join, relative } from "node:path"
import * as fs from "node:fs"
import { existsSync, readdirSync } from "node:fs"
import { execFileSync } from "node:child_process"
import { getError } from "get-error"
import { DEST_DIR, CONTRACT_DIR, GRPC_TOOLS_BASE_DIR } from "./paths"
import semver from "semver/preload"
import { glob } from "glob"

export async function compileProtoFiles (): Promise<void> {
  fs.mkdirSync(path.resolve(DEST_DIR, "ts"), { recursive: true })

  const protocPath = findProtocPath()
  console.log(`Using protoc from: ${protocPath}`)

  const protoFiles = await findProtoFiles()
  console.log(`Found ${protoFiles.length} proto file(s)`)

  const pluginPath = getProtocPluginPath()
  const grpcInclude = findGrpcToolsInclude()

  for (const protoFile of protoFiles) {
    // Use a path relative to CONTRACT_DIR so protoc accepts it with the --proto_path
    const relativePath = relative(CONTRACT_DIR, protoFile).replace(/\\/g, "/")
    console.log(`Compiling ${relativePath}...`)

    const args = buildProtocArgs(
      relativePath,
      pluginPath,
      grpcInclude,
    )

    try {
      execFileSync(protocPath, args, {
        cwd: CONTRACT_DIR,
        stdio: "inherit",
        encoding: "utf-8",
      })
    } catch (error) {
      console.error(`Failed to compile ${relativePath}`)
      const errorMessage = getError(error)
      if (errorMessage) {
        console.error(errorMessage)
      }
      process.exit(1)
    }
  }

  console.log("\x1b[32mSuccessfully compiled all proto files!\x1b[0m")
}

function findProtocPath (): string {
  // Try to find protoc in PATH first
  try {
    execFileSync("protoc", ["--version"], { encoding: "utf-8" })
    return "protoc"
  } catch {
    // Not in PATH, look in grpc.tools
  }

  // Find protoc from grpc.tools NuGet package
  if (!existsSync(GRPC_TOOLS_BASE_DIR)) {
    console.error("Error: protoc not found in PATH and grpc.tools package not found")
    console.error("Install protoc: https://grpc.io/docs/protoc-installation/")
    process.exit(1)
  }

  const versions = readdirSync(GRPC_TOOLS_BASE_DIR)
    .filter((v) => semver.valid(v))
    .sort((a, b) => semver.rcompare(a, b))

  if (versions.length === 0) {
    console.error("Error: No grpc.tools versions found")
    process.exit(1)
  }

  // grpc.tools packages protoc binaries per OS/architecture
  let protocSubdir: string
  if (process.platform === "win32") {
    protocSubdir = process.arch === "x64" ? "windows_x64" : "windows_x86"
  } else if (process.platform === "darwin") {
    protocSubdir = process.arch === "arm64" ? "macosx_arm64" : "macosx_x64"
  } else {
    // Linux
    protocSubdir = process.arch === "arm64" ? "linux_arm64" : "linux_x64"
  }

  const protocPath = join(
    GRPC_TOOLS_BASE_DIR,
    versions[0],
    "tools",
    protocSubdir,
    process.platform === "win32" ? "protoc.exe" : "protoc",
  )

  if (!existsSync(protocPath)) {
    console.error(`Error: protoc not found at ${protocPath}`)
    console.error("Make sure grpc.tools NuGet package is restored")
    process.exit(1)
  }

  return protocPath
}

/**
 * Finds all .proto files in the contract directory while skipping
 * dependency and build artifact directories
 */
async function findProtoFiles (): Promise<string[]> {
  const pattern = "**/*.proto"
  const ignore = ["**/node_modules/**", "**/bin/**", "**/obj/**"]

  return await glob(pattern, {
    cwd: CONTRACT_DIR,
    absolute: true,
    ignore,
  })
}

function getProtocPluginPath (): string {
  const isWindows = process.platform === "win32"
  return join(
    CONTRACT_DIR,
    "node_modules",
    ".bin",
    isWindows ? "protoc-gen-ts_proto.cmd" : "protoc-gen-ts_proto",
  )
}

function findGrpcToolsInclude (): string | undefined {
  if (!existsSync(GRPC_TOOLS_BASE_DIR)) {
    return undefined
  }

  const versions = readdirSync(GRPC_TOOLS_BASE_DIR)
    .filter((v) => semver.valid(v))
    .sort((a, b) => semver.rcompare(a, b))

  if (versions.length === 0) {
    return undefined
  }

  const includePath = join(
    GRPC_TOOLS_BASE_DIR,
    versions[0],
    "build",
    "native",
    "include",
  )

  return existsSync(includePath) ? includePath : undefined
}

function buildProtocArgs (
  protoFile: string,
  pluginPath: string,
  grpcInclude?: string,
): string[] {
  const args = [
    // Proto path options - order matters for resolution
    `--proto_path=${CONTRACT_DIR}`,
  ]

  const protoPath = process.env.PROTO_PATH
  if (protoPath) {
    args.push(`--proto_path=${protoPath}`)
  }

  if (grpcInclude) {
    args.push(`--proto_path=${grpcInclude}`)
  } else {
    console.warn(
      "Warning: grpc.tools include directory not found, google/protobuf imports may fail",
    )
  }

  // Use ts-proto in browser mode with JSON serialization, omitting client/server stubs
  args.push(
    `--plugin=${pluginPath}`,
    `--ts_proto_opt=esModuleInterop`,
    `--ts_proto_opt=env=browser`,
    `--ts_proto_opt=forceLong=string`,
    `--ts_proto_opt=outputEncodeMethods=false`,
    `--ts_proto_opt=outputJsonMethods=true`,
    `--ts_proto_opt=outputClientImpl=false`,
    `--ts_proto_opt=nestJs=false`,
    `--ts_proto_out=${path.resolve(DEST_DIR, "ts")}`,
    protoFile,
  )

  return args
}
