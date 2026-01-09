<template>
  <div class="flex flex-col items-center justify-center w-full md:w-[80vw] lg:w-[75vw]  p-4 md:p-6">
    <span class="text-4xl font-bold underline mb-2">{{ t('chart') }}</span>

    <div v-if="!userStore.isLoggedIn" class="text-gray-600 text-lg">
     {{ t('please_login') }}
    </div>

    <div v-else class="w-full flex flex-col md:flex-row md:space-x-6 space-y-6 md:space-y-0">

      <!-- 🔹 直條圖：各任務花費時間 -->
      <div class="bg-white p-4 rounded shadow w-full md:w-1/2">
        <h2 class="text-xl font-semibold mb-3">{{ t('chartTitle1') }}</h2>
        <BarChart v-if="rawData.length > 0" :key="rawData.length" :data="barChartData || { labels: [], datasets: [] }"
          :options="barChartOptions" />
        <div v-else class="text-center text-gray-500 mt-4">{{ t('nodata') }}</div>
      </div>

      <!-- 🔹 折線圖：每日完成任務數量 -->
      <div class="bg-white p-4 rounded shadow w-full md:w-1/2">
        <h2 class="text-xl font-semibold mb-3">{{ t('chartTitle2') }}</h2>
        <LineChart v-if="rawData.length > 0" :key="rawData.length" :data="lineChartData || { labels: [], datasets: [] }"
          :options="lineChartOptions" />
        <div v-else class="text-center text-gray-500 mt-4">{{ t('nodata') }}</div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { userStore } from '@/stores/user'

// Chart.js & Vue wrapper
import { Chart as ChartJS, Title, Tooltip, Legend, BarElement, LineElement, PointElement, CategoryScale, LinearScale, Filler } from 'chart.js'
import { Bar, Line } from 'vue-chartjs'

//語言
import { t } from '@/stores/lang'
// 註冊 Chart.js 元件
ChartJS.register(Title, Tooltip, Legend, BarElement, LineElement, PointElement, CategoryScale, LinearScale, Filler)

// -------------------- state --------------------
const rawData = ref([])

// -------------------- API --------------------
const fetchData = async () => {
  const memberId = userStore.memberId
  if (!memberId) return

  try {
    const res = await fetch(`/task/finished/${memberId}`)
    if (!res.ok) throw new Error('fetch failed')
    const data = await res.json()
    rawData.value = Array.isArray(data) ? data : []

  } catch (err) {
    console.error('Fetch chart data failed:', err)
    rawData.value = []
  }
}

onMounted(fetchData)

// -------------------- 直條圖（任務花費時間） --------------------
const barChartData = computed(() => {
  if (!rawData.value || rawData.value.length === 0)
    return { labels: [], datasets: [] }

  return {
    labels: rawData.value.map(item => item?.taskName ?? '(未命名任務)'), // ← 改這裡
    datasets: [
      {
        label: t('label1'),
        data: rawData.value.map(item => Math.round((item?.totalSeconds ?? 0) / 60)), // ← 一樣改小寫
        backgroundColor: '#60A5FA'
      }
    ]
  }
})

const barChartOptions = {
  responsive: true,
  plugins: {
    tooltip: {
      callbacks: {
        label: context => `花費時間: ${context.raw} 分鐘`
      }
    }
  },
  scales: {
    y: { beginAtZero: true }
  }
}

// -------------------- 折線圖（每日完成任務數量 + 任務名稱） --------------------
const lineChartData = computed(() => {
  if (!rawData.value || rawData.value.length === 0) return { labels: [], datasets: [] }

  const dailyMap = {}
  rawData.value.forEach(item => {

    const date = (item?.updateTime ?? '').substring(0, 10) || '未知日期'
    if (!dailyMap[date]) dailyMap[date] = { taskCount: 0, taskNames: [] }
    dailyMap[date].taskCount += 1
    dailyMap[date].taskNames.push(item?.taskName ?? '(未命名任務)')

  })

  const sortedDates = Object.keys(dailyMap).sort((a, b) => new Date(a) - new Date(b))

  return {
    labels: sortedDates,
    datasets: [
      {
        label: t('label2'),
        data: sortedDates.map(date => dailyMap[date].taskCount),
        borderColor: '#34D399',
        backgroundColor: '#34D39933',
        tension: 0.3,
        fill: true
      }
    ],
    _dailyMap: dailyMap
  }
})

const lineChartOptions = {
  responsive: true,
  plugins: {
    tooltip: {
      callbacks: {
        title: function (context) {
          return context[0].label
        },
        label: function (context) {
          return `完成任務數量：${context.raw}`
        },
        afterLabel: function (context) {
          const date = context.label
          const dailyMap = context.chart.data._dailyMap || {}
          const names = dailyMap[date]?.taskNames || []
          // 每個任務名稱顯示在新的一行
          return names.map(n => `• ${n}`).join('\n')
        }
      }
    },
    legend: {
      labels: { color: '#333' }
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        stepSize: 1,
        callback: value => (Number.isInteger(value) ? value : null)
      }
    },
    x: {
      ticks: { color: '#333' }
    }
  }
}


// -------------------- 組件別名 --------------------
const BarChart = Bar
const LineChart = Line
</script>

<style scoped>
.chart-wrapper {
  margin: auto;
}
</style>
