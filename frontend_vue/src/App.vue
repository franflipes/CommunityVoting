<template>
  <v-app>
    <v-app-bar v-if="showChrome" color="surface" border="b">
      <v-app-bar-title class="font-weight-bold">
        <v-icon icon="mdi-vote-outline" color="primary" class="mr-2" />
        CommunityVoting
      </v-app-bar-title>
      <v-spacer />
      <v-btn to="/communities" variant="text" prepend-icon="mdi-domain">Comunidades</v-btn>
      <v-btn v-if="authStore.isAdmin" to="/admin/settings" variant="text" prepend-icon="mdi-cog-outline">Administración</v-btn>
      <v-chip v-if="authStore.user" class="d-none d-md-flex" size="small" color="primary" variant="tonal">
        {{ authStore.user.name }} · {{ roleLabel }}
      </v-chip>
      <TooltipButton :icon="themeIcon" tooltip="Cambiar tema" variant="text" @click="toggleTheme" />
      <TooltipButton icon="mdi-logout" tooltip="Cerrar sesión" variant="text" @click="logout" />
    </v-app-bar>

    <v-main>
      <div class="workspace">
        <div v-if="authStore.loading" class="loading-page">
          <v-progress-circular indeterminate color="primary" />
          <span>Cargando aplicación...</span>
        </div>
        <router-view v-else />
      </div>
    </v-main>

    <v-footer v-if="showChrome" border="t" class="justify-center text-caption text-medium-emphasis">
      © {{ currentYear }} CommunityVoting · Votaciones comunitarias transparentes y seguras
    </v-footer>

    <ToastHost />
    <ConfirmDialogHost />
  </v-app>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTheme } from 'vuetify'
import { authStore } from '@/stores/auth'
import { services } from '@/services/api'
import { UserRole } from '@/constants'
import { saveTheme } from '@/theme'
import ToastHost from '@/components/ToastHost.vue'
import ConfirmDialogHost from '@/components/ConfirmDialogHost.vue'
import TooltipButton from '@/components/TooltipButton.vue'

const route = useRoute()
const router = useRouter()
const theme = useTheme()
const showChrome = computed(() => authStore.isAuthenticated && !route.meta.public)
const themeIcon = computed(() => theme.global.current.value.dark ? 'mdi-white-balance-sunny' : 'mdi-weather-night')
const currentYear = new Date().getFullYear()
const roleLabel = computed(() => {
  if (authStore.user?.role === UserRole.GlobalAdmin) return 'Administrador global'
  if (authStore.user?.role === UserRole.CommunityAdmin) return 'Administrador'
  return 'Votante'
})

onMounted(async () => {
  if (authStore.token) {
    try {
      authStore.user = await services.me()
    } catch {
      authStore.clear()
    }
  }
  authStore.loading = false
  if (route.meta.auth && !authStore.isAuthenticated) router.replace('/login')
  else if (route.meta.admin && !authStore.isAdmin) router.replace('/communities')
})

function toggleTheme() {
  const name = theme.global.current.value.dark ? 'communityLight' : 'communityDark'
  theme.change(name)
  saveTheme(name)
}

function logout() {
  authStore.clear()
  router.push('/login')
}
</script>
