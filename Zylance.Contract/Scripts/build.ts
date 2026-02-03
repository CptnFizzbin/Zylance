#!/usr/bin/env node

import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, readdirSync, rmSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import { glob } from "glob";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import { XMLParser } from "fast-xml-parser";

// Directory constants for key locations
const SCRIPT_DIR = dirname(new URL(import.meta.url).pathname);
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
    console.error("Error: OutputDir is required");
    console.error(
      "Usage: vite-node Scripts/build.ts <path>"
    );
    console.error(
      "   or: vite-node Scripts/build.ts --output-dir <path>"
    );
    process.exit(1);
  }

  return {
    outputDir: resolve(outputDir as string),
  };
}

function getOutputDirFromCsproj(): string | null {
  const csprojPath = join(CONTRACT_DIR, "Zylance.Contract.csproj");
  
  try {
    const content = readFileSync(csprojPath, "utf-8");
    const parser = new XMLParser();
    const parsed = parser.parse(content);
    
    // Navigate to find TsOutputPath property
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
    // If we can't parse, just return null and let the caller handle it
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

  const versions = readdirSync(GRPC_TOOLS_BASE_DIR).sort().reverse();
  if (versions.length === 0) {
    console.error("Error: No grpc.tools versions found");
    process.exit(1);
  }

  // Determine platform-specific protoc location
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

async function findProtoFiles(): Promise<string[]> {
  // Use glob to find all .proto files excluding node_modules, bin, and obj directories
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

  // Get latest version by sorting descending
  const versions = readdirSync(GRPC_TOOLS_BASE_DIR).sort().reverse();
  
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

  // Plugin and generation options
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

// Entry point
await main();
