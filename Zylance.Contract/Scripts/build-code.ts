#!/usr/bin/env vite-node

import { execFileSync } from "node:child_process"
import * as fs from "node:fs"
import { existsSync, readFileSync, readdirSync } from "node:fs"
import path, { dirname, join, relative, resolve } from "node:path"
import { fileURLToPath } from "node:url"
import { glob } from "glob"
import yargs from "yargs"
import { hideBin } from "yargs/helpers"
import { XMLParser } from "fast-xml-parser"
import semver from "semver"
import { getError } from "get-error"
import { generateConstants } from "./Lib/generate-constants"

const SCRIPT_DIR = dirname(fileURLToPath(import.meta.url))
const CONTRACT_DIR = resolve(SCRIPT_DIR, "..")
const DEST_DIR = resolve(CONTRACT_DIR, "Generated")
const HOME_DIR = process.env.HOME || process.env.USERPROFILE || ""
const GRPC_TOOLS_BASE_DIR = join(HOME_DIR, ".nuget", "packages", "grpc.tools")

interface BuildOptions {
  tsOutputDir: string;
}

async function main (): Promise<void> {
  const options = await parseCommandLineArgs()

  console.log(`Contract Directory: ${CONTRACT_DIR}`)
  if (options.tsOutputDir) {
    console.log(`Output Directory: ${options.tsOutputDir}`)
  }

  await compileProtoFiles(DEST_DIR)
  await generateConstants(DEST_DIR)

  if (options.tsOutputDir) {
    console.log("Copying generated TypeScript files to output directory...")
    fs.rmSync(options.tsOutputDir, { recursive: true, force: true })
    fs.cpSync(path.resolve(DEST_DIR, "ts"), options.tsOutputDir, { recursive: true })
  }
}

async function parseCommandLineArgs (): Promise<BuildOptions> {
  const argv = await yargs(hideBin(process.argv))
    .command("$0 [ts-output-path]", "Compile Protocol Buffer files to TypeScript", (yargs) => {
      yargs.positional("ts-output-path", {
        describe: "Output directory for generated TypeScript files",
        type: "string",
      })
    })
    .option("ts-output-path", {
      alias: ["o", "ts-output"],
      type: "string",
      description: "Output directory for generated TypeScript files",
    })
    .help()
    .parse()

  let tsOutputPath = argv.tsOutputPath

  // Fall back to reading from .csproj if not provided
  if (!tsOutputPath) {
    tsOutputPath = getTsOutputDirFromCsproj()
  }

  if (!tsOutputPath) {
    throw new Error(
      "TsOutputPath is required. Usage: vite-node Scripts/build.ts <path> or --ts-output-dir <path>",
    )
  }

  return {
    tsOutputDir: resolve(tsOutputPath),
  }
}

function getTsOutputDirFromCsproj (): string | undefined {
  const csprojPath = join(CONTRACT_DIR, "Zylance.Contract.csproj")

  try {
    const content = readFileSync(csprojPath, "utf-8")
    const parser = new XMLParser()
    const parsed = parser.parse(content)

    // MSBuild PropertyGroup can be a single object or an array depending on the .csproj structure
    const propertyGroups = parsed.Project?.PropertyGroup
    if (Array.isArray(propertyGroups)) {
      for (const group of propertyGroups) {
        if (group.TsOutputPath) {
          return group.TsOutputPath
        }
      }
    } else if (propertyGroups?.TsOutputPath) {
      return propertyGroups.TsOutputPath
    }
  } catch (error) {
    // If we can't read or parse the .csproj, treat it as "no configured output dir"
    // and let the caller enforce that an explicit output path is provided. We only
    // log unexpected errors; ENOENT (file not found) is silently ignored so the
    // script can run outside of a full .NET build context.
    const err = error as { code?: string; message?: string }
    if (err.code !== "ENOENT") {
      console.warn(
        `Warning: Failed to read or parse ${csprojPath}. ` +
        "Falling back to requiring an explicit --ts-output-dir. " +
        (err.message ? `Details: ${err.message}` : ""),
      )
    }
  }

  return undefined
}

async function compileProtoFiles (outputDir: string) {
  fs.mkdirSync(path.resolve(outputDir, "ts"), { recursive: true })

  const protocPath = findProtocPath()
  console.log(`Using protoc from: ${protocPath}`)

  const protoFiles = await findProtoFiles()
  console.log(`Found ${protoFiles.length} proto file(s)`)

  const pluginPath = getProtocPluginPath()
  const grpcInclude = findGrpcToolsInclude()

  for (const protoFile of protoFiles) {
    const relativePath = relative(CONTRACT_DIR, protoFile)
    console.log(`Compiling ${relativePath}...`)

    const args = buildProtocArgs(
      protoFile,
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

await main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error))
  process.exit(1)
})
