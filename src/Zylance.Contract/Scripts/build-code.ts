#!/usr/bin/env vite-node

import { generateConstants } from "./Lib/generate-constants"
import { compileProtoFiles } from "./Lib/compile-proto-files"
import { CONTRACT_DIR } from "./Lib/paths"

async function main (): Promise<void> {
  console.log(`Contract Directory: ${CONTRACT_DIR}`)
  await compileProtoFiles()
  await generateConstants()
}

await main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error))
  process.exit(1)
})
