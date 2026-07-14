#!/bin/bash
set -euo pipefail

# Only needed for Claude Code on the web; local dev machines set this up themselves.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

cd "$CLAUDE_PROJECT_DIR"

# --- .NET 10 SDK ---
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-sdks | grep -q '^10\.'; then
  apt-get update -qq
  apt-get install -y -qq dotnet-sdk-10.0
fi

# --- Node.js (project requires ^26; base image ships older LTS versions) ---
export NVM_DIR="/opt/nvm"
# shellcheck disable=SC1091
. "$NVM_DIR/nvm.sh"
nvm install 26 >/dev/null

NODE_HOME="$(dirname "$(dirname "$(nvm which 26)")")"
echo "export NODE_HOME=\"$NODE_HOME\"" >> "$CLAUDE_ENV_FILE"
echo "export PATH=\"$NODE_HOME/bin:\$PATH\"" >> "$CLAUDE_ENV_FILE"
export PATH="$NODE_HOME/bin:$PATH"

# --- Yarn (pinned per package.json "packageManager") ---
# The base image's bundled corepack (0.34.x) fails to fetch through this
# environment's proxy (undici ProxyAgent bug), so upgrade it first.
npm install -g corepack@latest --silent
corepack enable

# --- .NET dependencies and tools ---
dotnet restore
dotnet tool restore

# --- Build everything (also runs `yarn install` + protobuf/TS codegen for
#     Zylance.Contract and Zylance.UI via their MSBuild targets) ---
dotnet build --no-restore

# --- Playwright browsers (Zylance.Desktop.Tests E2E/smoke tests) ---
# Invoke the driver bundled in the restored NuGet package directly (rather than
# the generated bin/**/playwright.ps1) since this environment has no pwsh.
PLAYWRIGHT_PKG_DIR="$(find "$HOME/.nuget/packages/microsoft.playwright" -maxdepth 1 -mindepth 1 -type d 2>/dev/null | sort -V | tail -1)"
if [ -n "$PLAYWRIGHT_PKG_DIR" ]; then
  "$PLAYWRIGHT_PKG_DIR/.playwright/node/linux-x64/node" "$PLAYWRIGHT_PKG_DIR/.playwright/package/cli.js" install --with-deps chromium
fi
