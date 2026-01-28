import { defineConfig } from 'vite';
import { devtools } from '@tanstack/devtools-vite';
import viteReact from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

import { tanstackRouter } from '@tanstack/router-plugin/vite';
import { fileURLToPath } from 'node:url';
import * as path from 'node:path';

const ZYLANCE_ROOT = fileURLToPath(new URL('..', import.meta.url));
const ZYLANCE_UI_DIR = path.join(ZYLANCE_ROOT, 'Zylance.UI');
const ZYLANCE_DESKTOP_DIR = path.join(ZYLANCE_ROOT, 'Zylance.Desktop');

export default defineConfig({
  plugins: [
    devtools(),
    tanstackRouter({
      target: 'react',
      autoCodeSplitting: true,
      routesDirectory: path.join(ZYLANCE_UI_DIR, 'Src', 'Routes'),
      generatedRouteTree: path.join(ZYLANCE_UI_DIR, 'Src', 'routeTree.gen.ts'),
    }),
    viteReact({
      babel: {
        plugins: ['babel-plugin-react-compiler'],
      },
    }),
    tailwindcss(),
  ],

  root: ZYLANCE_UI_DIR,
  publicDir: path.join(ZYLANCE_UI_DIR, 'public'),

  build: {
    outDir: path.join(ZYLANCE_DESKTOP_DIR, 'Resources', 'wwwroot'),
    emptyOutDir: true,
    sourcemap: true
  },

  resolve: {
    alias: {
      '@': path.join(ZYLANCE_UI_DIR, 'Src'),
      '@Contract': path.join(ZYLANCE_UI_DIR, 'Generated/zylance'),
      '@Lib': path.join(ZYLANCE_UI_DIR, 'Lib'),
    },
  },
});
