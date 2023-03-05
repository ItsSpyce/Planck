import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  clearScreen: true,
  server: {
    strictPort: true,
    port: 3000,
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
  },
});
