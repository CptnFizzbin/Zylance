import { defineConfig } from 'vite';
import { devtools } from '@tanstack/devtools-vite';
import viteReact from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

import { tanstackRouter } from '@tanstack/router-plugin/vite';
import { fileURLToPath, URL } from 'node:url';
import * as path from 'node:path';

const ROOT_DIR = fileURLToPath(new URL('./Resources', import.meta.url));

export default defineConfig({
  root: ROOT_DIR,
  publicDir: path.join(ROOT_DIR, 'public'),
  build: {
    outDir: path.join(ROOT_DIR, 'wwwroot'),
  },
  plugins: [
    devtools(),
    tanstackRouter({
      target: 'react',
      autoCodeSplitting: true,
    }),
    viteReact({
      babel: {
        plugins: ['babel-plugin-react-compiler'],
      },
    }),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.join(ROOT_DIR, 'src'),
    },
  },
});
