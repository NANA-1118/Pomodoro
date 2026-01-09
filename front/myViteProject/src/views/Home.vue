<template>
  <div class="flex flex-col items-center justify-center w-full md:w-[80vw] lg:w-[75vw] h-[80vh] p-4 md:p-6 ">




    <!-- 主容器 -->
    <div class="flex flex-col md:flex-row w-full h-[80vh] items-center justify-center gap-4 md:gap-6">

      <!-- Timer 區塊 -->
      <div class="w-full md:w-1/2 flex flex-col items-center justify-start bg-gray-100 p-4 md:p-6 h-1/2 md:h-full">

        <!-- Timer 標題 -->
        <div class="bg-gray-700 text-white px-6 py-3 rounded-md mb-2 w-full text-center">
          {{ t('timer') }}
        </div>

        <!-- 模式切換 -->
        <div class="w-full bg-gray-700 text-white rounded-md mb-2 flex overflow-hidden">
          <div @click="setMode('Focus')" :class="mode === 'Focus' ? 'bg-red-500' : 'bg-gray-400'"
            class="flex-1 text-center py-2 cursor-pointer">
            {{ t('focus') }}
          </div>
          <div class="w-px bg-gray-600"></div>
          <div @click="setMode('Relax')" :class="mode === 'Relax' ? 'bg-red-500' : 'bg-gray-400'"
            class="flex-1 text-center py-2 cursor-pointer">
            {{ t('relax') }}
          </div>
        </div>
        <br>
        <br>

        <!-- 進度圓環 -->
        <ProgressCircle :timeLeft="timeLeft" :workTime="workTime" :isStopped="isStopped" />

        <!-- 控制按鈕 -->
        <div class="flex gap-10 mt-4">
          <!-- 顯示時機:初始/暫停時 -->
          <button v-if="!isRunning" @click="startTimer" class="w-12 h-12 bg-center bg-contain bg-no-repeat rounded"
            style="background-image: url('/image/start.png')">
          </button>

          <!-- 顯示時機:倒數中 -->
          <button v-if="isRunning" @click="stopTimer" class="w-12 h-12 bg-center bg-contain bg-no-repeat rounded"
            style="background-image: url('/image/pause.png')">
          </button>

          <!-- 顯示時機:倒數中/暫停時 -->
          <button v-if="isRunning || isStopped" @click="resetTimer"
            class="w-12 h-12 bg-center bg-contain bg-no-repeat rounded"
            style="background-image: url('/image/reset.png')">
          </button>
        </div>

      </div>

      <!-- Tasks 區塊 -->
      <div class="w-full md:w-1/2 flex flex-col bg-gray-100 p-4 md:p-6 h-1/2 md:h-full">

        <!-- Task 標題 -->
        <div class="bg-gray-700 text-white px-6 py-3 rounded-md mb-2 w-full text-center">
          {{ t('task') }}
        </div>

        <!-- ToDo / Done 切換 -->
        <div class="w-full bg-gray-700 text-white rounded-md mb-2 flex overflow-hidden">
          <div @click="tab = 'todo'" :class="tab === 'todo' ? 'bg-red-500' : 'bg-gray-400'"
            class="flex-1 text-center py-2 cursor-pointer">
            {{ t('todo') }}
          </div>
          <div class="w-px bg-gray-600"></div>
          <div @click="tab = 'done'" :class="tab === 'done' ? 'bg-red-500' : 'bg-gray-400'"
            class="flex-1 text-center py-2 cursor-pointer">
            {{ t('done') }}
          </div>
        </div>

        <!-- ToDo 列表 -->
        <div v-if="tab === 'todo'" class="flex-1 flex flex-col text-lg md:text-xl">
          <!-- 清單（可滾動區域） -->
          <ul class="flex-1 min-h-0 overflow-y-auto flex flex-col justify-start">
            <li v-for="task in todoTasks" :key="task.taskId"
              class="flex items-center p-2 border-b cursor-pointer relative"
              :class="selectedTask === task.taskId ? 'bg-blue-200 font-bold' : ''" @click="selectedTask = task.taskId">
              <!--checkbox-->
              <div class="absolute left-2">
                <input type="checkbox" class="w-6 h-6 mr-2 accent-pink-300" :value="task.taskId" v-model="selectedTasks"
                  @click.stop />
              </div>

              <!--任務名稱-->
              <div class="flex-1 text-center">
                {{ task.taskName }}
              </div>
            </li>
          </ul>

          <!-- 新增任務區塊（固定在底部，不會被捲走） -->
          <div class="flex flex-col gap-2 mt-2">
            <!-- 上層：只有勾選時才顯示 -->
            <div v-if="selectedTasks.length > 0" class="flex w-full bg-pink-200 rounded-md p-2 gap-2">
              <!-- 完成 -->
              <button @click="markAsDone" class="flex-1 h-12 bg-center bg-contain bg-no-repeat"
                style="background-image: url('/image/finish.png')">
              </button>

              <!-- 刪除 -->
              <button @click="deleteTasks" class="flex-1 h-12 bg-center bg-contain bg-no-repeat"
                style="background-image: url('/image/delete.png')">
              </button>
            </div>

            <!--  下層：新增任務輸入區 -->
            <div class="flex gap-2">
              <input v-model="newTaskName" type="text"  :placeholder="t('entertask')"
                class="flex-1 border rounded p-2 text-black" />
              <button @click="addTask" class="w-12 h-12 bg-center bg-contain bg-no-repeat rounded"
                style="background-image: url('/image/add.png')">
              </button>
            </div>
          </div>
        </div>


        <!-- Done 列表 -->
        <div v-else class="flex-1 min-h-0 overflow-y-auto text-lg md:text-xl">
          <ul class="h-full flex flex-col justify-start">
            <li v-for="task in doneTasks" :key="task.taskId" class="p-2 border-b">
              {{ task.taskName }} - {{ formatMinutes(task.totalSeconds) }}
            </li>
          </ul>
        </div>

      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue'
import ProgressCircle from '../components/ProgressCircle.vue'
import { userStore } from '@/stores/user'
//語言
import { t } from '@/stores/lang'
export default {
  components: { ProgressCircle },
  setup() {


    const todoTasks = ref([])
    const doneTasks = ref([])
    const selectedTask = ref(null)
    const selectedTasks = ref([])  // 用於多選任務
    const tab = ref('todo')

    const mode = ref('Focus')
    const workTime = computed(() => (mode.value === 'Focus' ? 25 * 60 : 5 * 60))
    const timeLeft = ref(workTime.value)

    const isRunning = ref(false)
    const isStopped = ref(false)
    let timer = null



    // 當模式改變時同步更新 timeLeft
    watch(workTime, (newVal) => {
      timeLeft.value = newVal
    })

    const fetchTasks = async () => {
      const memberId = userStore.memberId
      if (!memberId) {
        console.warn('尚未登入，無法載入任務')
        return
      }

      try {
        const todoRes = await fetch(`/task/unfinished/${memberId}`)
        todoTasks.value = await todoRes.json()

        const doneRes = await fetch(`/task/finished/${memberId}`)
        doneTasks.value = await doneRes.json()
      } catch (err) {
        console.error('Fetch tasks failed:', err)
      }
    }

    const formatMinutes = (seconds) => {
      const minutes = Math.floor(seconds / 60)
      const hours = Math.floor(minutes / 60)
      const remainMinutes = minutes % 60
      return hours > 0 ? `${hours}小時${remainMinutes}分鐘` : `${minutes}分鐘`
    }

    const setMode = (newMode) => {
      mode.value = newMode
      resetTimer()
    }

    const startTimer = () => {
      if (isRunning.value) return
      isRunning.value = true
      isStopped.value = false

      timer = setInterval(async () => {
        if (timeLeft.value > 0) {
          timeLeft.value--
        } else {
          clearInterval(timer)
          isRunning.value = false
          isStopped.value = true
          await addElapsedTimeToDb()  // 只有選取任務才會累計
          timeLeft.value = workTime.value
        }
      }, 1000)
    }



    const stopTimer = () => {
      if (timer) clearInterval(timer)
      isRunning.value = false
      isStopped.value = true
    }




    const resetTimer = async () => {
      await addElapsedTimeToDb()   // 重置前累計

      if (timer) clearInterval(timer)
      timeLeft.value = workTime.value
      isRunning.value = false
      isStopped.value = false
    }

    const addElapsedTimeToDb = async () => {
      if (mode.value !== 'Focus') return    // 只在 Focus 模式累計
      if (!selectedTask.value) return       // 沒選任務就跳過

      const elapsed = workTime.value - timeLeft.value
      if (elapsed <= 0) return              // 沒經過時間就跳過

      try {
        await fetch(`/task/addtime/${selectedTask.value}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(elapsed)
        })
      } catch (err) {
        console.error('Add elapsed time failed:', err)
      }
    }

    // 新增任務
    const newTaskName = ref('')
    const addTask = async () => {
      const memberId = userStore.memberId
      if (!memberId) {
        alert('請先登入再新增任務')
        return
      }

      if (!newTaskName.value.trim()) return // 空白不送

      try {
        const res = await fetch(`/task/addtask/${memberId}`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(newTaskName.value)
        })

        if (res.ok) {
          // 新增成功，重新拉取任務
          await fetchTasks()
          newTaskName.value = '' // 清空輸入框
        } else {
          console.error('新增任務失敗:', res.statusText)
        }
      } catch (err) {
        console.error('Add task failed:', err)
      }
    }

    // 完成任務 API
    const markAsDone = async () => {
      if (selectedTasks.value.length === 0) return

      try {
        const res = await fetch(`/task/finish`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(selectedTasks.value)
        })
        if (!res.ok) console.error('批次完成任務失敗')
        await fetchTasks()
        selectedTasks.value = []
      } catch (err) {
        console.error('批次完成任務失敗:', err)
      }
    }

    // 刪除任務 API
    const deleteTasks = async () => {
      if (selectedTasks.value.length === 0) return

      try {
        const res = await fetch(`/task/delete`, {
          method: 'POST',  // POST 批次刪除
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(selectedTasks.value)
        })
        if (!res.ok) console.error('批次刪除任務失敗')
        await fetchTasks()
        selectedTasks.value = []
      } catch (err) {
        console.error('批次刪除任務失敗:', err)
      }
    }



    //當組件載入完成後，自動執行 fetchTasks() 函式
    onMounted(fetchTasks)

    return {
      //語言
      t,

      // task 相關
      todoTasks,
      doneTasks,
      selectedTask,
      selectedTasks,
      tab,
      mode,
      workTime,
      timeLeft,
      isRunning,
      isStopped,
      startTimer,
      stopTimer,
      resetTimer,
      setMode,
      formatMinutes,
      newTaskName,
      addTask,
      markAsDone,
      deleteTasks
    }
  }
}
</script>
