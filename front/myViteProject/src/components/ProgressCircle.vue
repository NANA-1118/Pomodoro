<template>
  <div class="relative w-90 h-90 flex items-center justify-center">
    <!-- 背景圓環 -->
    <svg class="absolute w-full h-full" viewBox="0 0 100 100">
      <circle cx="50" cy="50" r="45" stroke="#ccc" stroke-width="10" fill="none" />
      <!-- 前景圓環 -->
      <circle cx="50" cy="50" r="45" :stroke="isStopped ? '#ccc' : 'red'" stroke-width="10" fill="none"
        stroke-linecap="round" :stroke-dasharray="circumference" :stroke-dashoffset="dashOffset"
        transform="rotate(-90 50 50)" class="transition-all duration-300" />
    </svg>

    <!-- 中間倒數時間 -->
    <div class="absolute text-6xl font-mono">
      {{ minutes }}:{{ seconds < 10 ? '0' + seconds : seconds }} </div>
    </div>
</template>

<script>
import { ref, computed, watch } from 'vue'

export default {
  props: {
    timeLeft: { type: Number, required: true },
    workTime: { type: Number, required: true },
    isStopped: { type: Boolean, required: true }
  },
  setup(props) {
    const radius = 45
    const circumference = 2 * Math.PI * radius

    const dashOffset = computed(() => {
      return circumference * (props.timeLeft / props.workTime)

    })

    const minutes = computed(() => Math.floor(props.timeLeft / 60))
    const seconds = computed(() => props.timeLeft % 60)

    return { circumference, dashOffset, minutes, seconds }
  }
}
</script>

<style scoped></style>
