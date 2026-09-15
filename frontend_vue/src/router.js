import { createRouter, createWebHistory } from 'vue-router'
import { authStore } from '@/stores/auth'

const routes = [
  { path: '/login', name: 'login', component: () => import('@/views/Login.vue'), meta: { public: true } },
  { path: '/meeting/access/:token', name: 'meeting-access', component: () => import('@/views/MeetingAccess.vue'), meta: { public: true } },
  { path: '/join', name: 'join', component: () => import('@/views/JoinCommunity.vue'), meta: { public: true } },
  { path: '/communities', name: 'communities', component: () => import('@/views/Communities.vue'), meta: { auth: true } },
  { path: '/communities/:id', name: 'community', component: () => import('@/views/CommunityDetail.vue'), meta: { auth: true } },
  { path: '/communities/:id/settings', name: 'community-settings', component: () => import('@/views/AdminSettings.vue'), meta: { auth: true, admin: true } },
  { path: '/admin/settings', name: 'admin-settings', component: () => import('@/views/AdminSettings.vue'), meta: { auth: true, admin: true } },
  { path: '/meetings/:id', name: 'meeting', component: () => import('@/views/MeetingDetail.vue'), meta: { auth: true } },
  { path: '/voting/:sessionId', name: 'voting', component: () => import('@/views/VotingRoom.vue'), meta: { auth: true } },
  { path: '/:pathMatch(.*)*', redirect: '/communities' }
]

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to) => {
  if (to.meta.auth && !authStore.isAuthenticated) return { path: '/login', query: { redirect: to.fullPath } }
  if (to.meta.admin && !authStore.loading && !authStore.isAdmin) return '/communities'
  if (to.name === 'login' && authStore.isAuthenticated) return '/communities'
})

window.addEventListener('auth:expired', () => {
  if (router.currentRoute.value.name !== 'login') router.push('/login')
})

export default router
