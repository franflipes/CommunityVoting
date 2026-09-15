<template>
  <div>
    <header class="page-header">
      <div>
        <h1 class="page-title">Comunidades registradas</h1>
        <p class="page-subtitle">Accede a las juntas y salas de votación privadas de tu comunidad.</p>
      </div>
      <v-btn v-if="authStore.isAdmin" color="primary" prepend-icon="mdi-plus" @click="dialog = true">Nueva comunidad</v-btn>
    </header>

    <div v-if="loading" class="loading-page"><v-progress-circular indeterminate color="primary" /></div>
    <v-alert v-else-if="error" type="error" variant="tonal">{{ error }}</v-alert>
    <v-card v-else-if="!communities.length" class="panel empty-state">
      <v-icon icon="mdi-domain-off" size="48" class="mb-3" />
      <h2 class="text-h6">No hay comunidades registradas</h2>
      <p>Pide a un administrador que te agregue o crea una comunidad si tienes permisos.</p>
    </v-card>
    <div v-else class="card-grid">
      <v-card v-for="community in communities" :key="community.id" class="panel" hover>
        <v-card-text>
          <div class="d-flex justify-space-between align-start mb-3">
            <v-avatar color="primary" variant="tonal"><v-icon icon="mdi-domain" /></v-avatar>
            <v-chip size="small" color="info" prepend-icon="mdi-shield-check-outline">Censo auditado</v-chip>
          </div>
          <h2 class="text-h6 font-weight-bold">{{ community.name }}</h2>
          <p class="muted">{{ community.address }}</p>
          <v-divider class="my-4" />
          <div class="text-body-2 muted"><v-icon icon="mdi-account-group-outline" size="small" /> {{ community.membersCount ?? community.memberCount ?? 0 }} miembros</div>
        </v-card-text>
        <v-card-actions><v-btn block variant="tonal" color="primary" :to="`/communities/${community.id}`" append-icon="mdi-arrow-right">Ver juntas y votaciones</v-btn></v-card-actions>
      </v-card>
    </div>

    <v-dialog v-model="dialog" max-width="520">
      <v-card class="panel">
        <DialogHeader icon="mdi-domain-plus" @close="dialog = false">Nueva comunidad</DialogHeader>
        <v-card-text>
          <v-alert v-if="formError" type="error" variant="tonal" class="mb-4">{{ formError }}</v-alert>
          <v-form @submit.prevent="create">
            <v-text-field v-model="form.name" label="Nombre de la comunidad" required />
            <v-text-field v-model="form.address" label="Dirección" required />
            <v-text-field v-model="form.cif" label="CIF / NIF (opcional)" />
            <div class="d-flex justify-end ga-2"><v-btn variant="text" @click="dialog = false">Cancelar</v-btn><v-btn color="primary" type="submit" :loading="saving">Crear comunidad</v-btn></div>
          </v-form>
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { authStore } from '@/stores/auth'
import { apiMessage, services } from '@/services/api'
import { useToasts } from '@/composables/useToasts'
import DialogHeader from '@/components/DialogHeader.vue'

const toast = useToasts()
const communities = ref([])
const loading = ref(true)
const error = ref('')
const dialog = ref(false)
const saving = ref(false)
const formError = ref('')
const form = reactive({ name: '', address: '', cif: '' })

onMounted(load)
async function load() {
  loading.value = true
  try { communities.value = await services.communities() }
  catch (err) { error.value = apiMessage(err, 'Error cargando las comunidades.') }
  finally { loading.value = false }
}
async function create() {
  saving.value = true
  formError.value = ''
  try {
    await services.createCommunity({ ...form })
    dialog.value = false
    Object.assign(form, { name: '', address: '', cif: '' })
    toast.success('Comunidad creada correctamente.')
    await load()
  } catch (err) { formError.value = apiMessage(err, 'Error al crear la comunidad.') }
  finally { saving.value = false }
}
</script>
