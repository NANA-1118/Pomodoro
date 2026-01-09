<template>
    <transition name="fade">
        <!-- 點背景才會關閉 -->
        <div v-if="isOpen" class="fixed inset-0 flex items-center justify-center bg-black/40 z-50"
            @click.self="$emit('close')">
            <div class="relative bg-white rounded-2xl p-8 shadow-xl w-full max-w-md">
                <!-- 左上角 返回箭頭 (signup / forgot 模式顯示) -->
                <button v-if="mode === 'signup' || mode === 'forgot'"
                    class="absolute top-3 left-3 text-gray-500 hover:text-gray-800 text-xl" @click="mode = 'login'">
                    ←
                </button>

                <!-- 右上角 X -->
                <button class="absolute top-3 right-3 text-gray-500 hover:text-gray-800 text-xl"
                    @click="$emit('close')">
                    ✕
                </button>

                <!-- 登入模式 -->
                <div v-if="mode === 'login'">
                    <h2 class="text-2xl font-bold mb-4 text-center">Sign in</h2>
                    <button class="w-full py-2 mb-4 border rounded-lg hover:bg-gray-100">
                        Continue with Google
                    </button>
                    <span>-------------------------------------------------------</span>

                    <!-- 登入錯誤訊息 -->
                    <div v-if="loginError" class="text-red-600 mb-2">{{ loginError }}</div>

                    <input type="email" v-model="loginEmail" placeholder="Email"
                        class="w-full p-2 border rounded mb-3" />
                    <input type="password" v-model="loginPassword" placeholder="Password"
                        class="w-full p-2 border rounded mb-3" />
                    <button @click="signIn" class="w-full py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
                        Sign in
                    </button>

                    <div class="flex justify-between text-sm mt-4">
                        <button class="text-blue-600 hover:underline" @click="mode = 'signup'">Sign up</button>
                        <button class="text-gray-600 hover:underline" @click="mode = 'forgot'">Forgot password?</button>
                    </div>
                </div>

                <!-- 註冊模式 -->
                <div v-else-if="mode === 'signup'">
                    <h2 class="text-2xl font-bold mb-4 text-center">Sign up</h2>

                    <!-- 顯示錯誤訊息 -->
                    <div v-if="signupError" class="text-red-600 mb-2">{{ signupError }}</div>

                    <input type="email" v-model="signupEmail" placeholder="Email"
                        class="w-full p-2 border rounded mb-3" />
                    <input type="password" v-model="signupPassword" placeholder="Password"
                        class="w-full p-2 border rounded mb-3" />
                    <button @click="signUp" class="w-full py-2 bg-green-600 text-white rounded hover:bg-green-700">
                        Sign up
                    </button>
                </div>

                <!-- 忘記密碼模式 -->
                <div v-else-if="mode === 'forgot'">
                    <h2 class="text-2xl font-bold mb-4 text-center">Forgot password</h2>

                    <!-- 顯示成功或錯誤訊息 -->
                    <div v-if="forgotError" class="text-red-600 mb-2">{{ forgotError }}</div>
                    <div v-if="forgotSuccess" class="text-green-600 mb-2">{{ forgotSuccess }}</div>

                    <input type="email" v-model="forgotEmail" placeholder="Email"
                        class="w-full p-2 border rounded mb-3" />

                    <button @click="sendNewPassword"
                        class="w-full py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600">
                        Send new password
                    </button>
                </div>

            </div>
        </div>
    </transition>
</template>

<script setup>
import { ref } from "vue"
import { userStore } from '../stores/user'
defineProps({
    isOpen: Boolean
})

const emit = defineEmits(['close'])
const mode = ref('login') // login | signup | forgot


// 登入資料
const loginEmail = ref("")
const loginPassword = ref("")
const loginError = ref('')

// 註冊資料
const signupEmail = ref("")
const signupPassword = ref("")
const signupError = ref("")

// 忘記密碼資料
const forgotEmail = ref("")
const forgotError = ref("") // 錯誤訊息


async function signUp() {
    signupError.value = "" // 清空錯誤訊息
    console.log("Signing up with", signupEmail.value, signupPassword.value)
    if (!signupEmail.value || !signupPassword.value) {
        signupError.value = "Account or Password cannot be empty"
        return
    }

    try {
        const res = await fetch("/member/signup", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                Account: signupEmail.value,
                Password: signupPassword.value
            })
        })

        if (res.status === 409) { // Conflict
            signupError.value = "已存在相同帳號"
        } else if (res.ok) {
            alert("註冊成功")
            signupEmail.value = ""
            signupPassword.value = ""
            signupError.value = ""
            mode.value = "login"
        } else {
            signupError.value = "註冊失敗"
        }
    } catch (err) {
        console.error(err)
        signupError.value = "註冊發生錯誤"

    }
}
async function signIn() {
    loginError.value = "" // 清空錯誤訊息

    if (!loginEmail.value || !loginPassword.value) {
        loginError.value = "Account or Password cannot be empty"
        return
    }

    try {
        const res = await fetch("/member/signin", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                Account: loginEmail.value,
                Password: loginPassword.value
            })
        })

        const data = await res.json()

        if (res.ok) {
            alert(`登入成功，歡迎 ${data.memberName || data.memberId}`)
            // 存到 localStorage
            localStorage.setItem('user', JSON.stringify({
                memberId: data.memberId,
                memberName: data.memberName,
            }))

            // 更新全局狀態
            userStore.isLoggedIn = true
            userStore.memberId = data.memberId
            userStore.memberName = data.memberName


            loginEmail.value = ""
            loginPassword.value = ""
            loginError.value = ""
            mode.value = "login" // 可以關閉 modal 或做其他跳轉
            emit('close') // 登入成功自動關閉 modal
        } else {
            loginError.value = data.message || "登入失敗"
        }
    } catch (err) {
        console.error(err)
        loginError.value = "登入發生錯誤"
    }
}

async function sendNewPassword() {
    forgotError.value = ""

    if (!forgotEmail.value) {
        forgotError.value = "Email cannot be empty"
        return
    }

    try {
        const res = await fetch("/member/forgot", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Email: forgotEmail.value })
        })

        const data = await res.json()

        if (res.ok) {
            forgotError.value = data.message || "新密碼已寄送至您的信箱"
            forgotEmail.value = ""
        } else {
            forgotError.value = data.message || "寄送失敗，請確認 Email 是否正確"
        }
    } catch (err) {
        console.error(err)
        forgotError.value = "發送過程中發生錯誤"
    }
}
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
