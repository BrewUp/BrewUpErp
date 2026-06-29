import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@features': path.resolve(__dirname, './src/features'),
      '@shared': path.resolve(__dirname, './src/shared'),
    },
  },
  server: {
    proxy: {
      '/v1': {
        target: process.env.VITE_API_BASE_URL ?? 'http://localhost:6094',
        changeOrigin: true,
      },
      '/hubs': {
        target: process.env.VITE_API_BASE_URL ?? 'http://localhost:6094',
        ws: true,
        changeOrigin: true,
      },
    },
  },
})
