import { reactive } from 'vue'
import { UserRole } from '@/constants'

const TOKEN_KEY = 'cv_token'

export const authStore = reactive({
  token: localStorage.getItem(TOKEN_KEY) || '',
  user: null,
  loading: true,
  get isAuthenticated() {
    return Boolean(this.token)
  },
  get isAdmin() {
    return this.user?.role === UserRole.GlobalAdmin || this.user?.role === UserRole.CommunityAdmin
  },
  setSession(data) {
    this.token = data?.token || data?.accessToken || ''
    this.user = data?.user || null
    if (this.token) localStorage.setItem(TOKEN_KEY, this.token)
    else localStorage.removeItem(TOKEN_KEY)
  },
  clear() {
    this.token = ''
    this.user = null
    localStorage.removeItem(TOKEN_KEY)
  }
})
