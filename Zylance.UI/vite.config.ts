import * as path from "node:path"
import { devtools } from "@tanstack/devtools-vite"
import { tanstackRouter } from "@tanstack/router-plugin/vite"
import viteReact from "@vitejs/plugin-react"
import { defineConfig } from "vite"

const ZYLANCE_UI_DIR = import.meta.dirname

// Custom plugin to replace websocket URL placeholder in index.html
function injectWebSocketUrl() {
  return {
    name: "inject-websocket-url",
    transformIndexHtml(html: string) {
      const wsPort = process.env.ZYLANCE_WS_PORT
      if (!wsPort) return html

      console.log("Injecting WebSocket URL with port:", wsPort)
      return html.replace(
        "{{zylance.webSocketUrl}}",
        `ws://localhost:${wsPort}`,
      )
    },
  }
}

export default defineConfig({
  plugins: [
    injectWebSocketUrl(),
    devtools(),
    tanstackRouter({
      target: "react",
      autoCodeSplitting: true,
      routesDirectory: path.join(ZYLANCE_UI_DIR, "Src", "Routes"),
      generatedRouteTree: path.join(ZYLANCE_UI_DIR, "Src", "routeTree.gen.ts"),
    }),
    viteReact({
      babel: {
        plugins: ["babel-plugin-react-compiler"],
      },
    }),
  ],

  root: ZYLANCE_UI_DIR,
  publicDir: path.join(ZYLANCE_UI_DIR, "Public"),

  build: {
    outDir: path.join(ZYLANCE_UI_DIR, "dist"),
    emptyOutDir: true,
    sourcemap: true,
  },

  resolve: {
    alias: {
      "@": path.join(ZYLANCE_UI_DIR, "Src"),
      "@Contract": path.join(ZYLANCE_UI_DIR, "Generated", "zylance"),
      $Generated: path.join(ZYLANCE_UI_DIR, "Generated"),
      "@Lib": path.join(ZYLANCE_UI_DIR, "Lib"),
    },
  },
})
