import { reactive } from 'vue'

export const langStore = reactive({
  currentLang: 'en', // 預設 English
  setLang(lang) {
    this.currentLang = lang
  }
})

// 字典
export const messages = {
  en: {
    home: 'Home',
    chart: 'Chart',
    logout: 'Logout',
    signin: 'Sign in',
    signin_desc: 'to save focus history and tasks.',
    timer: 'Timer',
    focus: 'Focus',
    relax: 'Relax',
    task: 'Task',
    todo: 'ToDo',
    done: 'Done',
    memberId: 'memberId',
    chartTitle1: 'Time spent on each task (minutes)',
    chartTitle2: 'Number of tasks completed daily',
    nodata: 'No task statistics available',
    please_login: 'Please login to view chart',
    label1: 'Total time (minutes)',
    label2: 'Number of tasks completed',
    entertask: 'Enter task name',
    pomodoro: 'Pomodoro Timer',
    },
  zh: {
    home: '首頁',
    chart: '統計圖表',
    logout: '登出',
    signin: '登入',
    signin_desc: '以儲存專注歷史與任務',
    timer: '計時器',
    focus: '專注',
    relax: '休息',
    task: '任務',
    todo: '待辦',
    done: '完成',
    memberId: '會員編號',
    chartTitle1: '各任務花費時間（分鐘）',
    chartTitle2: '每日完成任務數量',
    nodata: '尚無每日統計資料',
    please_login: '請先登入以查看統計圖表',
    label1: '總時間（分鐘）',
    label2: '完成任務數量',        
    entertask: '輸入任務名稱',
    pomodoro: '番茄鐘',
  }
}

// 取得文字
export function t(key) {
  return messages[langStore.currentLang][key] || key
}
