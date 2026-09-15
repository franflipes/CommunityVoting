<template>
  <div class="auth-shell">
    <v-card class="auth-card panel" elevation="8">
      <v-card-text class="pa-7">
        <div class="text-center mb-6">
          <div class="logo-mark"><v-icon icon="mdi-account-multiple-plus-outline" size="32" /></div>
          <h1 class="text-h5 font-weight-bold">Unirse a la comunidad</h1>
        </div>

        <div v-if="verifying" class="text-center py-8">
          <v-progress-circular indeterminate color="primary" />
          <div class="mt-3 muted">Verificando enlace de invitación...</div>
        </div>
        <v-alert v-else-if="!verification?.isValid" type="error" variant="tonal">
          <div class="font-weight-bold mb-1">Enlace inválido o expirado</div>
          {{ verification?.errorMessage || 'La invitación no es válida.' }}
          <div class="mt-3"><v-btn to="/login" variant="outlined" size="small">Ir al inicio de sesión</v-btn></div>
        </v-alert>
        <template v-else>
          <v-alert type="success" variant="tonal" class="mb-5">
            Invitación verificada para <strong>{{ verification.communityName }}</strong>
          </v-alert>
          <v-alert v-if="error" type="error" variant="tonal" class="mb-4">{{ error }}</v-alert>
          <v-form @submit.prevent="submit">
            <v-row dense>
              <v-col cols="12" sm="6"><v-text-field v-model="form.name" label="Nombre" required /></v-col>
              <v-col cols="12" sm="6"><v-text-field v-model="form.lastName" label="Apellidos" required /></v-col>
            </v-row>
            <v-text-field v-model="form.email" label="Correo electrónico" type="email" required />
            <v-text-field v-model="form.phoneNumber" label="Teléfono (opcional)" />
            <v-text-field v-model="form.password" label="Contraseña" type="password" minlength="6" required />
            <v-btn block color="primary" size="large" type="submit" :loading="loading">Completar registro y entrar</v-btn>
          </v-form>
        </template>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authStore } from '@/stores/auth'
import { apiMessage, services } from '@/services/api'

const route = useRoute()
const router = useRouter()
const token = String(route.query.token || '')
const verification = ref(null)
const verifying = ref(true)
const loading = ref(false)
const error = ref('')
const form = reactive({ name: '', lastName: '', email: '', password: '', phoneNumber: '' })

onMounted(async () => {
  if (!token) {
    verification.value = { isValid: false, errorMessage: 'No se ha especificado ningún token de invitación.' }
    verifying.value = false
    return
  }
  try {
    verification.value = await services.verifyInvitation(token)
  } catch {
    verification.value = { isValid: false, errorMessage: 'No se pudo verificar la invitación.' }
  } finally {
    verifying.value = false
  }
})

async function submit() {
  loading.value = true
  error.value = ''
  try {
    const session = await services.registerWithInvitation({ token, ...form })
    authStore.setSession(session)
    await router.push(verification.value.communityId ? `/communities/${verification.value.communityId}` : '/communities')
  } catch (err) {
    error.value = apiMessage(err, 'Error completando el registro en la comunidad.')
  } finally {
    loading.value = false
  }
}
</script>
