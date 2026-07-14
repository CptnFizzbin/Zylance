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
# Corepack's default download host (repo.yarnpkg.com) is blocked by this
# environment's egress policy, so install the pinned Yarn release directly
# from npm instead of going through corepack.
YARN_VERSION="$(node -pe "require('./src/Zylance.UI/package.json').packageManager.split('@')[1]")"
if [ "$(yarn --version 2>/dev/null || true)" != "$YARN_VERSION" ]; then
  rm -f "$NODE_HOME/bin/yarn" "$NODE_HOME/bin/yarnpkg"
  npm install -g "@yarnpkg/cli-dist@$YARN_VERSION" --silent
fi

# --- .NET dependencies and tools ---
dotnet restore
dotnet tool restore

# --- Build everything (also runs `yarn install` + protobuf/TS codegen for
#     Zylance.Contract and Zylance.UI via their MSBuild targets) ---
dotnet build --no-restore

# --- Runtime deps for running/screenshotting Zylance.Desktop (Photino.NET) ---
# Photino.NET embeds a WebView backed by WebKitGTK on Linux, and its native
# lib also links libnotify; the base image doesn't ship either. scrot
# captures the Xvfb framebuffer for screenshots. xdotool resizes/positions
# the window: with SetUseOsDefaultSize/Location and no window manager on
# Xvfb, Photino's window opens tiny (~200x200) in the top-left corner.
# Xvfb itself is already present on the base image.
if ! ldconfig -p | grep -q libwebkit2gtk-4.1; then
  apt-get update -qq
  apt-get install -y -qq libwebkit2gtk-4.1-0 libnotify4 scrot xdotool
fi
