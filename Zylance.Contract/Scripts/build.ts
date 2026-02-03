#!/usr/bin/env node

import { execSync, execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readdirSync, rmSync, statSync } from "node:fs";
import { join, resolve, relative, isAbsolute, dirname } from "node:path";
import { fileURLToPath } from "node:url";

/**
 * Build script to compile Protocol Buffer files to TypeScript using ts-proto
 * Replaces the PowerShell build.ps1 script
 */

// Parse command line arguments
const args = process.argv.slice(2);
let outputDir = "";

for (let i = 0; i < args.length; i++) {
  if (args[i] === "--output-dir" && i + 1 < args.length) {
    outputDir = args[i + 1];
    i++;
  } else if (!outputDir && !args[i].startsWith("--")) {
    outputDir = args[i];
  }
}

if (!outputDir) {
  console.error("Error: OutputDir is required");
  console.error("Usage: vite-node Scripts/build.ts --output-dir <path>");
  console.error("   or: vite-node Scripts/build.ts <path>");
  process.exit(1);
}

// Check if protoc is available
try {
  const isWindows = process.platform === "win32";
  const whichCommand = isWindows ? "where protoc" : "which protoc";
  const protocPath = execSync(whichCommand, { encoding: "utf-8" }).trim();
  console.log(`protoc found at: ${protocPath}`);
} catch {
  console.error("Error: protoc not found in PATH");
  process.exit(1);
}

const protoPath = process.env.PROTO_PATH;
console.log(`PROTO_PATH: ${protoPath || "(not set)"}`);

// Determine directories
const scriptPath = fileURLToPath(import.meta.url);
const contractDir = resolve(dirname(scriptPath), "..");
console.log(`Contract Directory: ${contractDir}`);

// Convert relative path to absolute if needed
const outDir = isAbsolute(outputDir)
  ? outputDir
  : resolve(contractDir, outputDir);

console.log(`Output Directory: ${outDir}`);

// Find all .proto files recursively
function findProtoFiles(dir: string): string[] {
  const files: string[] = [];

  function traverse(currentDir: string) {
    const entries = readdirSync(currentDir);

    for (const entry of entries) {
      const fullPath = join(currentDir, entry);
      const stat = statSync(fullPath);

      if (stat.isDirectory()) {
        traverse(fullPath);
      } else if (entry.endsWith(".proto")) {
        files.push(fullPath);
      }
    }
  }

  traverse(dir);
  return files;
}

const protoFiles = findProtoFiles(contractDir);

if (protoFiles.length === 0) {
  console.error(`No .proto files found in ${contractDir}`);
  process.exit(1);
}

console.log(`Found ${protoFiles.length} proto file(s)`);

// Clean and recreate output directory
if (existsSync(outDir)) {
  rmSync(outDir, { recursive: true, force: true });
}
mkdirSync(outDir, { recursive: true });

// Determine the protoc plugin path
const isWindows = process.platform === "win32";
const pluginPath = join(
  contractDir,
  "node_modules",
  ".bin",
  isWindows ? "protoc-gen-ts_proto.cmd" : "protoc-gen-ts_proto"
);

// Compile each proto file
let hasErrors = false;

for (const protoFile of protoFiles) {
  const relativePath = relative(contractDir, protoFile);
  console.log(`Compiling ${relativePath}...`);

  // Build protoc command
  const args = [
    `--plugin=${pluginPath}`,
    `--ts_proto_opt=esModuleInterop`,
    `--ts_proto_opt=env=browser`,
    `--ts_proto_opt=forceLong=string`,
    `--ts_proto_opt=outputEncodeMethods=false`,
    `--ts_proto_opt=outputJsonMethods=true`,
    `--ts_proto_opt=outputClientImpl=false`,
    `--ts_proto_opt=nestJs=false`,
    `--ts_proto_out=${outDir}`,
    protoFile,
  ];

  // Add proto_path options
  if (protoPath) {
    args.unshift(`--proto_path=${protoPath}`);
  }
  args.unshift(`--proto_path=${contractDir}`);

  try {
    execFileSync("protoc", args, {
      cwd: contractDir,
      stdio: "inherit",
      encoding: "utf-8",
    });
  } catch (error) {
    console.error(
      `Failed to compile ${relative(contractDir, protoFile)}`
    );
    hasErrors = true;
    break;
  }
}

if (hasErrors) {
  process.exit(1);
}

console.log("\x1b[32mSuccessfully compiled all proto files!\x1b[0m");
