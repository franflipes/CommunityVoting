<template>
  <div v-if="loading" class="loading-page"><v-progress-circular indeterminate color="primary" /></div>
  <v-alert v-else-if="error || !community" type="error" variant="tonal">{{ error || 'Comunidad no encontrada.' }}</v-alert>
  <div v-else>
    <v-breadcrumbs :items="breadcrumbs" class="px-0" />
    <v-card class="panel mb-7">
      <v-card-text class="pa-6">
        <div class="page-header mb-0">
          <div>
            <v-chip size="small" color="info">CIF: {{ community.cif || 'No especificado' }}</v-chip>
            <h1 class="page-title mt-3">{{ community.name }}</h1>
            <p class="page-subtitle"><v-icon icon="mdi-map-marker-outline" size="small" /> {{ community.address }}</p>
          </div>
          <div v-if="authStore.isAdmin" class="d-flex flex-wrap ga-2">
            <v-btn variant="tonal" prepend-icon="mdi-cog-outline" :to="`/communities/${community.id}/settings`">Configurar</v-btn>
            <v-btn color="primary" prepend-icon="mdi-calendar-plus" @click="meetingDialog = true">Convocar junta</v-btn>
          </div>
        </div>
      </v-card-text>
    </v-card>

    <div class="responsive-grid">
      <section>
        <h2 class="section-title"><v-icon icon="mdi-calendar-month-outline" color="primary" /> Juntas convocadas</h2>
        <v-card v-if="!meetings.length" class="panel empty-state">No hay juntas convocadas.</v-card>
        <div v-else class="d-grid">
          <v-card v-for="meeting in meetings" :key="meeting.id" class="panel mb-3" hover>
            <v-card-text>
              <div class="d-flex justify-space-between">
                <v-chip size="small" color="info">{{ meeting.type === 0 ? 'Junta ordinaria' : 'Junta extraordinaria' }}</v-chip>
                <span class="text-caption muted">{{ formatDate(meeting.scheduledAt) }}</span>
              </div>
              <h3 class="text-h6 font-weight-bold mt-3">{{ meeting.title }}</h3>
              <p class="muted">{{ meeting.location }}</p>
            </v-card-text>
            <v-card-actions><v-btn block variant="tonal" :to="`/meetings/${meeting.id}`">Ver orden del día y votaciones</v-btn></v-card-actions>
          </v-card>
        </div>
      </section>
      <section>
        <h2 class="section-title"><v-icon icon="mdi-account-group-outline" color="primary" /> Censo ({{ members.length }})</h2>
        <v-card class="panel">
          <v-list bg-color="transparent">
            <v-list-item v-for="member in members" :key="member.id" :title="`${member.userName} ${member.userLastName || ''}`" :subtitle="member.userEmail" prepend-icon="mdi-account-circle">
              <template #append><v-chip size="small" :color="member.memberRole === 1 ? 'success' : 'info'">{{ member.memberRole === 1 ? 'Administrador' : 'Votante' }}</v-chip></template>
            </v-list-item>
            <v-list-item v-if="!members.length" title="No hay miembros en el censo." />
          </v-list>
        </v-card>
      </section>
    </div>

    <v-dialog v-model="meetingDialog" max-width="580">
      <v-card class="panel">
        <DialogHeader icon="mdi-calendar-plus" @close="meetingDialog = false">Convocar nueva junta</DialogHeader>
        <v-card-text>
          <v-alert v-if="formError" type="error" variant="tonal" class="mb-4">{{ formError }}</v-alert>
          <v-form @submit.prevent="createMeeting">
            <v-text-field v-model="form.title" label="Título" required />
            <v-select v-model="form.type" :items="meetingTypes" label="Tipo de convocatoria" />
            <v-text-field v-model="form.location" label="Ubicación / plataforma" required />
            <v-text-field v-model="form.scheduledAt" label="Primera convocatoria" type="datetime-local" required />
            <v-text-field v-model="form.secondCallAt" label="Segunda convocatoria (opcional)" type="datetime-local" />
            <div class="d-flex justify-end ga-2"><v-btn variant="text" @click="meetingDialog = false">Cancelar</v-btn><v-btn color="primary" type="submit" :loading="saving">Crear reunión</v-btn></div>
          </v-form>
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { authStore } from '@/stores/auth'
import { apiMessage, services } from '@/services/api'
import { MeetingType } from '@/constants'
import { useToasts } from '@/composables/useToasts'
import DialogHeader from '@/components/DialogHeader.vue'

const route = useRoute()
const toast = useToasts()
const id = computed(() => route.params.id)
const community = ref(null)
const meetings = ref([])
const members = ref([])
const loading = ref(true)
const error = ref('')
const meetingDialog = ref(false)
const saving = ref(false)
const formError = ref('')
const form = reactive({ title: '', type: MeetingType.Ordinary, location: 'Sala de Comunidad / Online', scheduledAt: '', secondCallAt: '' })
const meetingTypes = [{ title: 'Ordinaria', value: 0 }, { title: 'Extraordinaria', value: 1 }]
const breadcrumbs = computed(() => [{ title: 'Comunidades', to: '/communities' }, { title: community.value?.name || '' }])

onMounted(load)
async function load() {
  loading.value = true
  try {
    ;[community.value, meetings.value, members.value] = await Promise.all([services.community(id.value), services.meetings(id.value), services.members(id.value)])
  } catch (err) { error.value = apiMessage(err, 'Error cargando la comunidad.') }
  finally { loading.value = false }
}
async function createMeeting() {
  saving.value = true
  formError.value = ''
  try {
    await services.createMeeting({
      communityId: id.value,
      title: form.title,
      type: Number(form.type),
      location: form.location,
      scheduledAt: new Date(form.scheduledAt).toISOString(),
      secondCallAt: form.secondCallAt ? new Date(form.secondCallAt).toISOString() : null,
      isTransparent: true
    })
    meetingDialog.value = false
    toast.success('Reunión creada correctamente.')
    await load()
  } catch (err) { formError.value = apiMessage(err, 'Error al crear la reunión.') }
  finally { saving.value = false }
}
function formatDate(value) { return new Date(value).toLocaleString('es-ES') }
</script>
