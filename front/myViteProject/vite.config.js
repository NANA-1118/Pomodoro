import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import path from 'path' 

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    port: 5500,// 固定開發伺服器 port
    proxy: {
      '/task': {
        target: 'https://localhost:7215',// 目標後端網址+port號
        changeOrigin: true,
        secure: false,
      },
      '/member': {
        target: 'https://localhost:7215',// 目標後端網址+port號
        changeOrigin: true,
        secure: false,
      },
    },
  },
   resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  }
})
