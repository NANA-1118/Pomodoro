<template>
  <div class="flex flex-col min-h-screen w-full">
    <!-- 導航列 -->
    <nav class="p-6 w-full flex justify-between items-center">
      <div>
        <router-link to="/" class="mr-4">{{ t('home') }}</router-link>
        <router-link to="/Chart">{{ t('chart') }}</router-link>
      </div>

      <!-- 右上角會員區塊 -->
      <div class="flex items-center space-x-3 relative">
        <!-- Language 下拉選單 -->
        <div class="relative" @click.stop>
          <button @click="showLangDropdown = !showLangDropdown"
            class="flex items-center px-3 py-1 text-sm bg-gray-200 rounded hover:bg-gray-300 transition">
            Language
            <svg :class="{ 'rotate-180': showLangDropdown }" class="ml-2 w-4 h-4 transition-transform" fill="none"
              stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          <transition name="fade-slide">
            <div v-if="showLangDropdown"
              class="absolute right-0 mt-2 w-28 bg-white border rounded shadow-lg z-50"
              @click.outside="showLangDropdown = false">
              <button @click="switchToEn"
                :class="['w-full text-left px-3 py-2 hover:bg-gray-100', langStore.currentLang === 'en' ? 'font-bold' : '']">
                English
              </button>
              <button @click="switchToZh"
                :class="['w-full text-left px-3 py-2 hover:bg-gray-100', langStore.currentLang === 'zh' ? 'font-bold' : '']">
                中文
              </button>
            </div>
          </transition>
        </div>

        <!-- 已登入 -->
        <template v-if="userStore.isLoggedIn">
          <span class="text-gray-700 mr-3">
            {{ t('memberId') }} {{ userStore.memberId }}
          </span>
          <button @click="logout" class="px-3 py-1 text-sm bg-red-500 text-white rounded hover:bg-red-600">
            {{ t('logout') }}
          </button>
        </template>

        <!-- 未登入 -->
        <div v-else class="text-sm">
          <a @click="showLogin = true" class="text-blue-600 underline cursor-pointer">{{ t('signin') }}</a>
          <span class="ml-1">{{ t('signin_desc') }}</span>
        </div>
      </div>
    </nav>

    <!-- 標題 -->
    <h1 class="text-2xl font-bold mb-6 text-center w-full">
      🍅  {{ t('pomodoro') }}
    </h1>

    <!-- router-view 水平垂直置中 -->
    <div class="flex justify-center items-center w-full">
      <router-view class="w-full flex justify-center items-center" />
    </div>

    <!-- 登入彈窗 -->
    <LoginModal :isOpen="showLogin" @close="showLogin = false" />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { userStore } from '@/stores/user'
import { langStore, t } from '@/stores/lang'
import LoginModal from '@/components/LoginModal.vue'

// 語言下拉
const showLangDropdown = ref(false)
function switchToZh() { langStore.setLang('zh'); showLangDropdown.value = false }
function switchToEn() { langStore.setLang('en'); showLangDropdown.value = false }

// ------------------初始化全局狀態------------------
const savedUser = localStorage.getItem('user')
if (savedUser) {
  const user = JSON.parse(savedUser)
  userStore.isLoggedIn = true
  userStore.memberId = user.memberId
  userStore.memberName = user.memberName
}

// ------------------ 登入視窗控制 ------------------
const showLogin = ref(false)
function openLogin() { showLogin.value = true }

// ------------------登出方法------------------
function logout() {
  localStorage.removeItem('user')
  userStore.isLoggedIn = false
  userStore.memberId = null
  userStore.memberName = ""
}
</script>

<style scoped>
/* 下拉選單動畫 */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.2s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(-5px);
}
.fade-slide-enter-to {
  opacity: 1;
  transform: translateY(0);
}
.fade-slide-leave-from {
  opacity: 1;
  transform: translateY(0);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}
</style>
