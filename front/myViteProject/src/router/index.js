import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Chart from '../views/Chart.vue'

// 定義路由表
const routes = [
  { path: '/', name: 'Home', component: Home },
  { path: '/Chart', name: 'Chart', component: Chart }
]

// 建立 router
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL), // Vite 的設定
  routes
})

export default router
