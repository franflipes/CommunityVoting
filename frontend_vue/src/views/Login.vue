<template>
  <div class="auth-shell">
    <v-card class="auth-card panel" elevation="8">
      <v-card-text class="pa-7">
        <div class="text-center mb-6">
          <div class="logo-mark"><v-icon icon="mdi-vote-outline" size="34" /></div>
          <h1 class="text-h5 font-weight-bold">{{ title }}</h1>
          <p class="muted mt-1">{{ subtitle }}</p>
        </div>

        <v-alert v-if="error" type="error" variant="tonal" class="mb-4">{{ error }}</v-alert>
        <v-alert v-if="success" type="success" variant="tonal" class="mb-4">{{ success }}</v-alert>

        <v-form @submit.prevent="submit">
          <v-row v-if="mode === 'register'" dense>
            <v-col cols="12" sm="6"><v-text-field v-model="form.name" label="Nombre" required /></v-col>
            <v-col cols="12" sm="6"><v-text-field v-model="form.lastName" label="Apellidos" required /></v-col>
            <v-col cols="12"><v-text-field v-model="form.phoneNumber" label="Teléfono" /></v-col>
            <v-col cols="12">
              <v-select v-model="form.role" :items="roles" label="Rol inicial" />
            </v-col>
          </v-row>
          <v-text-field v-model="form.email" label="Correo electrónico" type="email" required />
          <v-text-field v-if="mode !== 'reset'" v-model="form.password" label="Contraseña" type="password" required />
          <v-text-field v-else v-model="form.newPassword" label="Nueva contraseña" type="password" minlength="6" required />
          <div v-if="mode === 'login'" class="text-right mb-3">
            <v-btn variant="text" size="small" @click="setMode('reset')">¿Olvidaste tu contraseña?</v-btn>
          </div>
          <v-btn block color="primary" size="large" type="submit" :loading="loading">
            {{ mode === 'register' ? 'Registrarse' : mode === 'reset' ? 'Guardar nueva contraseña' : 'Entrar' }}
          </v-btn>
        </v-form>

        <div class="text-center mt-4">
          <v-btn v-if="mode === 'reset'" variant="text" prepend-icon="mdi-arrow-left" @click="setMode('login')">Volver a iniciar sesión</v-btn>
          <v-btn v-else variant="text" @click="setMode(mode === 'login' ? 'register' : 'login')">
            {{ mode === 'register' ? '¿Ya tienes cuenta? Inicia sesión' : '¿No tienes cuenta? Regístrate aquí' }}
          </v-btn>
        </div>

        <template v-if="mode === 'login'">
          <v-divider class="my-5" />
          <div class="text-caption text-center text-medium-emphasis mb-3">ACCESO RÁPIDO DEMO</div>
          <v-row dense>
            <v-col cols="6"><v-btn block variant="tonal" :loading="loading" @click="quickLogin('laura@comunidad.com', 'Admin123!')">Laura (Admin)</v-btn></v-col>
            <v-col cols="6"><v-btn block variant="tonal" color="success" :loading="loading" @click="quickLogin('juan@comunidad.com', 'Voter123!')">Juan (Votante)</v-btn></v-col>
          </v-row>
        </template>
      </v-card-text>
    </v-card>
  </div>
</template>

<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { services, apiMessage } from '@/services/api'
import { authStore } from '@/stores/auth'
import { UserRole } from '@/constants'

const router = useRouter()
const route = useRoute()
const mode = ref('login')
const loading = ref(false)
const error = ref('')
const success = ref('')
const form = reactive({ name: '', lastName: '', phoneNumber: '', email: '', password: '', newPassword: '', role: UserRole.CommunityMember })
const roles = [
  { title: 'Votante / Miembro', value: UserRole.CommunityMember },
  { title: 'Administrador de comunidad', value: UserRole.CommunityAdmin }
]
const title = computed(() => mode.value === 'register' ? 'Crear cuenta' : mode.value === 'reset' ? 'Restablecer contraseña' : 'Iniciar sesión')
const subtitle = computed(() => mode.value === 'register' ? 'Regístrate para participar en las votaciones' : mode.value === 'reset' ? 'Introduce tu correo y tu nueva contraseña' : 'Accede a la plataforma de votación de tu comunidad')

function setMode(value) {
  mode.value = value
  error.value = ''
  success.value = ''
}

async function submit() {
  loading.value = true
  error.value = ''
  success.value = ''
  try {
    if (mode.value === 'register') {
      authStore.setSession(await services.register({ ...form, role: Number(form.role) }))
      await router.push(route.query.redirect || '/communities')
    } else if (mode.value === 'login') {
      authStore.setSession(await services.login({ email: form.email, password: form.password }))
      await router.push(route.query.redirect || '/communities')
    } else {
      const data = await services.resetPassword({ email: form.email, newPassword: form.newPassword })
      setMode('login')
      success.value = data.message || 'Contraseña restablecida correctamente.'
    }
  } catch (err) {
    error.value = apiMessage(err, 'Error de autenticación. Comprueba tus datos.')
  } finally {
    loading.value = false
  }
}

async function quickLogin(email, password) {
  form.email = email
  form.password = password
  await submit()
}
</script>
