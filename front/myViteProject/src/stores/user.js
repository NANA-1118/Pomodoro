import { reactive } from 'vue'

export const userStore = reactive({
    isLoggedIn: false,
    memberId: null,
    memberName: ''
})