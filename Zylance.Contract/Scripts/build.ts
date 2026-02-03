#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, readdirSync, rmSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { glob } from "glob";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import { XMLParser } from "fast-xml-parser";
import semver from "semver";
import getError from "get-error";

const SCRIPT_DIR = dirname(fileURLToPath(import.meta.url));
const CONTRACT_DIR = resolve(SCRIPT_DIR, "..");
const HOME_DIR = process.env.HOME || process.env.USERPROFILE || "";
const GRPC_TOOLS_BASE_DIR = join(HOME_DIR, ".nuget", "packages", "grpc.tools");

interface BuildOptions {
  outputDir: string;
}

async function main(): Promise<void> {
  const options = await parseCommandLineArgs();
  
  console.log(`Contract Directory: ${CONTRACT_DIR}`);
  console.log(`Output Directory: ${options.outputDir}`);

  const protocPath = findProtocPath();
  console.log(`Using protoc from: ${protocPath}`);
  
  const protoFiles = await findProtoFiles();
  console.log(`Found ${protoFiles.length} proto file(s)`);
  
  prepareOutputDirectory(options.outputDir);
  
  const pluginPath = getProtocPluginPath();
  const grpcInclude = findGrpcToolsInclude();
  
  compileProtoFiles(protoFiles, options.outputDir, pluginPath, grpcInclude, protocPath);
  
  console.log("\x1b[32mSuccessfully compiled all proto files!\x1b[0m");
}

async function parseCommandLineArgs(): Promise<BuildOptions> {
  const argv = await yargs(hideBin(process.argv))
    .command("$0 [output-dir]", "Compile Protocol Buffer files to TypeScript", (yargs) => {
      yargs.positional("output-dir", {
        describe: "Output directory for generated TypeScript files",
        type: "string",
      });
    })
    .option("output-dir", {
      alias: ["o", "output"],
      type: "string",
      description: "Output directory for generated TypeScript files",
    })
    .help()
    .parse();

  let outputDir = argv.outputDir || argv._[0];

  // Fall back to reading from .csproj if not provided
  if (!outputDir) {
    outputDir = getOutputDirFromCsproj();
  }

  if (!outputDir) {
    throw new Error(
      "OutputDir is required. Usage: vite-node Scripts/build.ts <path> or --output-dir <path>"
    );
  }

  return {
    outputDir: resolve(outputDir),
  };
}

function getOutputDirFromCsproj(): string | null {
  const csprojPath = join(CONTRACT_DIR, "Zylance.Contract.csproj");
  
  try {
    const content = readFileSync(csprojPath, "utf-8");
    const parser = new XMLParser();
    const parsed = parser.parse(content);
    
    // MSBuild PropertyGroup can be a single object or an array depending on the .csproj structure
    const propertyGroups = parsed.Project?.PropertyGroup;
    if (Array.isArray(propertyGroups)) {
      for (const group of propertyGroups) {
        if (group.TsOutputPath) {
          return group.TsOutputPath;
        }
      }
    } else if (propertyGroups?.TsOutputPath) {
      return propertyGroups.TsOutputPath;
    }
  } catch (error) {
    // If we can't read or parse the .csproj, treat it as "no configured output dir"
    // and let the caller enforce that an explicit output path is provided. We only
    // log unexpected errors; ENOENT (file not found) is silently ignored so the
    // script can run outside of a full .NET build context.
    const err = error as { code?: string; message?: string };
    if (err.code !== "ENOENT") {
      console.warn(
        `Warning: Failed to read or parse ${csprojPath}. ` +
          "Falling back to requiring an explicit --output-dir. " +
          (err.message ? `Details: ${err.message}` : "")
      );
    }
  }
  
  return null;
}

function findProtocPath(): string {
  // Try to find protoc in PATH first
  try {
    execFileSync("protoc", ["--version"], { encoding: "utf-8" });
    return "protoc";
  } catch {
    // Not in PATH, look in grpc.tools
  }

  // Find protoc from grpc.tools NuGet package
  if (!existsSync(GRPC_TOOLS_BASE_DIR)) {
    console.error("Error: protoc not found in PATH and grpc.tools package not found");
    console.error("Install protoc: https://grpc.io/docs/protoc-installation/");
    process.exit(1);
  }

  const versions = readdirSync(GRPC_TOOLS_BASE_DIR)
    .filter((v) => semver.valid(v))
    .sort((a, b) => semver.rcompare(a, b));

  if (versions.length === 0) {
    console.error("Error: No grpc.tools versions found");
    process.exit(1);
  }

  // grpc.tools packages protoc binaries per OS/architecture
  let protocSubdir: string;
  if (process.platform === "win32") {
    protocSubdir = process.arch === "x64" ? "windows_x64" : "windows_x86";
  } else if (process.platform === "darwin") {
    protocSubdir = process.arch === "arm64" ? "macosx_arm64" : "macosx_x64";
  } else {
    // Linux
    protocSubdir = process.arch === "arm64" ? "linux_arm64" : "linux_x64";
  }

  const protocPath = join(
    GRPC_TOOLS_BASE_DIR,
    versions[0],
    "tools",
    protocSubdir,
    process.platform === "win32" ? "protoc.exe" : "protoc"
  );

  if (!existsSync(protocPath)) {
    console.error(`Error: protoc not found at ${protocPath}`);
    console.error("Make sure grpc.tools NuGet package is restored");
    process.exit(1);
  }

  return protocPath;
}

/**
 * Finds all .proto files in the contract directory while skipping
 * dependency and build artifact directories
 */
async function findProtoFiles(): Promise<string[]> {
  const pattern = "**/*.proto";
  const ignore = ["**/node_modules/**", "**/bin/**", "**/obj/**"];
  
  return await glob(pattern, {
    cwd: CONTRACT_DIR,
    absolute: true,
    ignore,
  });
}

function prepareOutputDirectory(outDir: string): void {
  if (existsSync(outDir)) {
    rmSync(outDir, { recursive: true, force: true });
  }
  mkdirSync(outDir, { recursive: true });
}

function getProtocPluginPath(): string {
  const isWindows = process.platform === "win32";
  return join(
    CONTRACT_DIR,
    "node_modules",
    ".bin",
    isWindows ? "protoc-gen-ts_proto.cmd" : "protoc-gen-ts_proto"
  );
}

function findGrpcToolsInclude(): string | null {
  if (!existsSync(GRPC_TOOLS_BASE_DIR)) {
    return null;
  }

  const versions = readdirSync(GRPC_TOOLS_BASE_DIR)
    .filter((v) => semver.valid(v))
    .sort((a, b) => semver.rcompare(a, b));

  if (versions.length === 0) {
    return null;
  }

  const includePath = join(
    GRPC_TOOLS_BASE_DIR,
    versions[0],
    "build",
    "native",
    "include"
  );
  
  return existsSync(includePath) ? includePath : null;
}

function compileProtoFiles(
  protoFiles: string[],
  outDir: string,
  pluginPath: string,
  grpcInclude: string | null,
  protocPath: string
): void {
  const protoPath = process.env.PROTO_PATH;

  for (const protoFile of protoFiles) {
    const relativePath = relative(CONTRACT_DIR, protoFile);
    console.log(`Compiling ${relativePath}...`);

    const args = buildProtocArgs(
      protoFile,
      outDir,
      pluginPath,
      protoPath,
      grpcInclude
    );

    try {
      execFileSync(protocPath, args, {
        cwd: CONTRACT_DIR,
        stdio: "inherit",
        encoding: "utf-8",
      });
    } catch (error) {
      console.error(`Failed to compile ${relativePath}`);
      const errorMessage = getError(error);
      if (errorMessage) {
        console.error(errorMessage);
      }
      process.exit(1);
    }
  }
}

function buildProtocArgs(
  protoFile: string,
  outDir: string,
  pluginPath: string,
  protoPath: string | undefined,
  grpcInclude: string | null
): string[] {
  const args = [
    // Proto path options - order matters for resolution
    `--proto_path=${CONTRACT_DIR}`,
  ];

  if (protoPath) {
    args.push(`--proto_path=${protoPath}`);
  }

  if (grpcInclude) {
    args.push(`--proto_path=${grpcInclude}`);
  } else {
    console.warn(
      "Warning: grpc.tools include directory not found, google/protobuf imports may fail"
    );
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
    `--ts_proto_out=${outDir}`,
    protoFile
  );

  return args;
}

await main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
});
