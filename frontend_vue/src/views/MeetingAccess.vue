<template>
  <div class="auth-shell">
    <v-card class="auth-card panel" elevation="8">
      <v-card-text class="pa-7">
        <div class="text-center mb-6">
          <div class="logo-mark"><v-icon icon="mdi-key-variant" size="32" /></div>
          <h1 class="text-h5 font-weight-bold">Acceso a reunión</h1>
          <p class="muted mt-2">Introduce el código que has recibido junto a tu enlace de reunión.</p>
        </div>
        <v-alert v-if="error" type="error" variant="tonal" class="mb-4">{{ error }}</v-alert>
        <v-form @submit.prevent="submit">
          <v-text-field
            v-model="code"
            label="Código de acceso"
            prepend-inner-icon="mdi-key-variant"
            maxlength="10"
            class="access-code"
            required
          />
          <v-btn block size="large" color="primary" type="submit" :loading="loading">Entrar a la reunión</v-btn>
        </v-form>
        <v-divider class="my-5" />
        <div class="text-center text-body-2">
          <v-btn variant="text" to="/login">Usar contraseña habitual</v-btn>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authStore } from '@/stores/auth'
import { apiMessage, services } from '@/services/api'

const route = useRoute()
const router = useRouter()
const code = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  if (code.value.trim().length < 4) {
    error.value = 'Por favor, introduce tu código de acceso.'
    return
  }
  loading.value = true
  error.value = ''
  try {
    const data = await services.meetingAccess({ token: route.params.token, code: code.value.trim() })
    authStore.setSession({ token: data.accessToken, user: data.user })
    await router.push(data.redirectUrl || '/communities')
  } catch (err) {
    error.value = apiMessage(err, 'Error al validar la credencial de acceso. Comprueba el código.')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
:deep(.access-code input) { text-align: center; letter-spacing: .18em; font-size: 1.25rem; font-weight: 700; }
</style>
