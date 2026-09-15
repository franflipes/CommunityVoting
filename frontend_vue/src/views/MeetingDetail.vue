<template>
  <div v-if="loading" class="loading-page"><v-progress-circular indeterminate color="primary" /></div>
  <v-alert v-else-if="error || !meeting" type="error" variant="tonal">{{ error || 'Reunión no encontrada.' }}</v-alert>
  <div v-else>
    <v-breadcrumbs :items="breadcrumbs" class="px-0" />
    <v-card class="panel mb-6">
      <v-card-text class="pa-6">
        <div class="page-header mb-5">
          <div>
            <v-chip size="small" color="info">{{ meeting.type === 0 ? 'Junta ordinaria' : 'Junta extraordinaria' }}</v-chip>
            <h1 class="page-title mt-3">{{ meeting.title }}</h1>
            <p class="page-subtitle">{{ meeting.communityName }}</p>
          </div>
          <div v-if="authStore.isAdmin" class="d-flex flex-wrap ga-2">
            <v-btn variant="tonal" prepend-icon="mdi-key-variant" @click="openAccesses">Accesos</v-btn>
            <v-btn variant="tonal" prepend-icon="mdi-pencil-outline" @click="openMeetingEdit">Editar</v-btn>
            <v-btn variant="tonal" prepend-icon="mdi-format-list-numbered" @click="agendaDialog = true">Añadir punto</v-btn>
            <v-btn color="primary" prepend-icon="mdi-plus" @click="openProposal()">Nueva propuesta</v-btn>
          </div>
        </div>
        <div class="meta-grid">
          <div class="meta-item"><v-icon icon="mdi-calendar-clock" color="primary" /><div><small class="muted">Convocatoria</small><div>{{ formatDate(meeting.scheduledAt) }}</div></div></div>
          <div class="meta-item"><v-icon icon="mdi-map-marker-outline" color="primary" /><div><small class="muted">Ubicación</small><div>{{ meeting.location }}</div></div></div>
          <div class="meta-item"><v-icon icon="mdi-calendar-end-outline" color="primary" /><div><small class="muted">Cierre de votaciones</small><div>{{ formatDate(meeting.votingEnd) }}</div></div></div>
        </div>
      </v-card-text>
    </v-card>

    <v-card v-if="quorum" class="panel mb-7">
      <v-card-title class="d-flex flex-wrap align-center ga-2">
        <v-icon icon="mdi-account-group-outline" color="primary" /> Quórum y asistencia
        <v-spacer />
        <v-chip :color="quorum.quorumReached ? 'success' : 'warning'">{{ quorum.quorumReached ? 'Quórum alcanzado' : 'Quórum incompleto' }}</v-chip>
      </v-card-title>
      <v-card-text>
        <div class="meta-grid mb-4">
          <div><small class="muted">Miembros con voto</small><div class="text-h6">{{ quorum.eligibleMembers }}</div></div>
          <div><small class="muted">Presentes</small><div class="text-h6">{{ quorum.presentMembers }}</div></div>
          <div><small class="muted">Mínimo requerido</small><div class="text-h6">{{ quorum.quorumRequired }}</div></div>
        </div>
        <v-progress-linear :model-value="attendancePercentage" :color="quorum.quorumReached ? 'success' : 'warning'" height="10" rounded />
      </v-card-text>
      <v-card-actions class="flex-wrap">
        <v-btn :color="present ? 'success' : 'primary'" :prepend-icon="present ? 'mdi-check' : 'mdi-account-check-outline'" @click="recordAttendance">{{ present ? 'Asistencia confirmada' : 'Marcar mi asistencia' }}</v-btn>
        <v-btn variant="text" @click="openParticipants">Ver asistentes ({{ quorum.presentMembers }})</v-btn>
        <v-btn v-if="authStore.isAdmin" variant="text" @click="openSettings">Configurar reglas</v-btn>
      </v-card-actions>
    </v-card>

    <h2 class="section-title"><v-icon icon="mdi-format-list-numbered" color="primary" /> Orden del día ({{ meeting.agendaItems?.length || 0 }} puntos)</h2>
    <v-card v-if="!meeting.agendaItems?.length" class="panel empty-state">
      <v-icon icon="mdi-format-list-bulleted-square" size="44" class="mb-3" />
      <div class="text-h6">Aún no hay puntos en el orden del día.</div>
      <v-btn v-if="authStore.isAdmin" color="primary" class="mt-4" @click="agendaDialog = true">Crear primer punto</v-btn>
    </v-card>
    <div v-else class="d-grid ga-4">
      <v-card v-for="(item, index) in meeting.agendaItems" :key="item.id" class="panel mb-4">
        <v-card-title class="d-flex flex-wrap align-start ga-2">
          <div>
            <v-chip size="x-small" color="info">Punto {{ item.order || index + 1 }}</v-chip>
            <div class="mt-2">{{ item.title }}</div>
            <div v-if="item.description" class="text-body-2 muted">{{ item.description }}</div>
          </div>
          <v-spacer />
          <v-btn v-if="authStore.isAdmin" size="small" variant="tonal" prepend-icon="mdi-plus" @click="openProposal(item.id)">Propuesta</v-btn>
          <v-btn v-if="authStore.isAdmin" icon="mdi-delete-outline" size="small" variant="text" color="error" @click="deleteAgenda(item)" />
        </v-card-title>
        <v-card-text>
          <v-alert v-if="!item.proposals?.length" type="info" variant="tonal">Sin propuestas asignadas.</v-alert>
          <v-card v-for="proposal in item.proposals" :key="proposal.id" variant="outlined" class="mb-3">
            <v-card-text>
              <div class="d-flex flex-wrap align-start ga-2">
                <div class="flex-grow-1">
                  <div class="text-h6">{{ proposal.title }}</div>
                  <p v-if="proposal.description" class="muted">{{ proposal.description }}</p>
                  <div class="d-flex flex-wrap ga-1 mt-2"><v-chip v-for="option in proposal.options" :key="option.id" size="small">{{ option.label }}</v-chip></div>
                </div>
                <v-chip v-if="status(proposal).state === VotingState.Open" color="success" size="small">En vivo</v-chip>
                <v-chip v-if="status(proposal).state === VotingState.Closed" color="warning" size="small">Finalizada</v-chip>
                <v-btn v-if="authStore.isAdmin && ![VotingState.Open, VotingState.Closed].includes(status(proposal).state)" size="small" variant="tonal" prepend-icon="mdi-pencil" @click="openProposal(item.id, proposal)">Editar</v-btn>
                <v-btn v-if="authStore.isAdmin && status(proposal).state !== VotingState.Closed" size="small" color="primary" prepend-icon="mdi-play" @click="startVoting(proposal)">{{ status(proposal).state === VotingState.Open ? 'Ir a sala' : 'Iniciar' }}</v-btn>
                <v-btn v-if="status(proposal).state === VotingState.Closed || (!authStore.isAdmin && status(proposal).state === VotingState.Open)" size="small" :color="status(proposal).state === VotingState.Open ? 'success' : 'info'" :to="`/voting/${status(proposal).sessionId || proposal.id}`">{{ status(proposal).state === VotingState.Open ? 'Entrar a votar' : 'Ver resultado' }}</v-btn>
              </div>
              <v-divider class="my-4" />
              <div class="d-flex align-center flex-wrap ga-2 mb-2">
                <strong class="text-body-2">Documentos ({{ proposal.documents?.length || 0 }})</strong>
                <v-spacer />
                <v-btn v-if="authStore.isAdmin" size="small" variant="text" prepend-icon="mdi-paperclip" @click="openUpload(proposal.id)">Adjuntar</v-btn>
              </div>
              <v-list v-if="proposal.documents?.length" bg-color="transparent" density="compact">
                <v-list-item v-for="doc in proposal.documents" :key="doc.id" :title="doc.title" :subtitle="`${doc.fileName} · ${(doc.fileSize / 1024).toFixed(1)} KB`" prepend-icon="mdi-file-document-outline">
                  <template #append>
                    <v-btn icon="mdi-download" variant="text" size="small" @click="download(doc)" />
                    <v-btn v-if="authStore.isAdmin" icon="mdi-delete-outline" variant="text" color="error" size="small" @click="deleteDocument(doc)" />
                  </template>
                </v-list-item>
              </v-list>
              <div v-else class="text-caption muted">No hay documentos adjuntos.</div>
            </v-card-text>
          </v-card>
        </v-card-text>
      </v-card>
    </div>

    <v-dialog v-model="agendaDialog" max-width="540">
      <v-card class="panel"><DialogHeader icon="mdi-format-list-numbered" @close="agendaDialog = false">Nuevo punto del orden del día</DialogHeader><v-card-text>
        <v-form @submit.prevent="createAgenda">
          <v-text-field v-model="agendaForm.title" label="Título" required />
          <v-textarea v-model="agendaForm.description" label="Descripción o notas" rows="3" />
          <v-text-field v-model.number="agendaForm.order" label="Orden" type="number" min="1" required />
          <DialogActions :loading="saving" @cancel="agendaDialog = false" />
        </v-form>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="proposalDialog" max-width="600">
      <v-card class="panel"><DialogHeader icon="mdi-file-document-edit-outline" @close="proposalDialog = false">{{ editingProposal ? 'Editar propuesta' : 'Nueva propuesta' }}</DialogHeader><v-card-text>
        <v-form @submit.prevent="saveProposal">
          <v-select v-model="proposalForm.agendaItemId" :items="agendaItems" label="Punto del orden del día" required />
          <v-text-field v-model="proposalForm.title" label="Título" required />
          <v-textarea v-model="proposalForm.description" label="Descripción" rows="3" />
          <div class="text-subtitle-2 mb-2">Opciones de votación</div>
          <div v-for="(_option, index) in proposalForm.options" :key="index" class="d-flex ga-2">
            <v-text-field v-model="proposalForm.options[index]" :label="`Opción ${index + 1}`" required />
            <v-btn v-if="proposalForm.options.length > 2" icon="mdi-delete-outline" color="error" variant="text" @click="proposalForm.options.splice(index, 1)" />
          </div>
          <v-btn variant="tonal" prepend-icon="mdi-plus" class="mb-4" @click="proposalForm.options.push('')">Añadir opción</v-btn>
          <DialogActions :loading="saving" @cancel="proposalDialog = false" />
        </v-form>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="uploadDialog" max-width="540">
      <v-card class="panel"><DialogHeader icon="mdi-cloud-upload-outline" @close="uploadDialog = false">Adjuntar documento</DialogHeader><v-card-text>
        <v-form @submit.prevent="uploadDocument">
          <v-file-input v-model="uploadForm.file" label="Archivo" required show-size />
          <v-text-field v-model="uploadForm.title" label="Título" required />
          <v-textarea v-model="uploadForm.description" label="Descripción" rows="2" />
          <DialogActions :loading="saving" @cancel="uploadDialog = false" />
        </v-form>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="meetingDialog" max-width="580">
      <v-card class="panel"><DialogHeader icon="mdi-calendar-edit" @close="meetingDialog = false">Editar datos y fechas</DialogHeader><v-card-text>
        <v-form @submit.prevent="saveMeeting">
          <v-text-field v-model="meetingForm.title" label="Título" required />
          <v-select v-model="meetingForm.type" :items="meetingTypes" label="Tipo" />
          <v-text-field v-model="meetingForm.location" label="Ubicación" required />
          <v-text-field v-model="meetingForm.scheduledAt" label="Primera convocatoria" type="datetime-local" required />
          <v-text-field v-model="meetingForm.secondCallAt" label="Segunda convocatoria" type="datetime-local" />
          <v-switch v-model="meetingForm.isTransparent" label="Mostrar asistencia y quórum en tiempo real" color="primary" />
          <DialogActions :loading="saving" @cancel="meetingDialog = false" />
        </v-form>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="settingsDialog" max-width="580">
      <v-card class="panel"><DialogHeader icon="mdi-shield-check-outline" @close="settingsDialog = false">Reglas de quórum y mayoría</DialogHeader><v-card-text>
        <v-form @submit.prevent="saveSettings">
          <v-switch v-model="settingsForm.quorumEnabled" label="Exigir quórum" color="primary" />
          <v-text-field v-if="settingsForm.quorumEnabled" v-model.number="settingsForm.quorumPercentage" label="Porcentaje de quórum" type="number" suffix="%" />
          <v-switch v-if="settingsForm.quorumEnabled" v-model="settingsForm.requireQuorumForVoting" label="Impedir votación sin quórum" color="primary" />
          <v-select v-model="settingsForm.defaultMajorityType" :items="majorityTypes" label="Tipo de mayoría" />
          <v-text-field v-if="settingsForm.defaultMajorityType === 3" v-model.number="settingsForm.defaultMajorityPercentage" label="Porcentaje cualificado" type="number" suffix="%" />
          <v-select v-model="settingsForm.abstentionPolicy" :items="abstentionPolicies" label="Tratamiento de abstenciones" />
          <DialogActions :loading="saving" @cancel="settingsDialog = false" />
        </v-form>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="participantsDialog" max-width="720">
      <v-card class="panel"><DialogHeader icon="mdi-account-check-outline" @close="participantsDialog = false">Registro de asistencia</DialogHeader><v-card-text>
        <v-table><thead><tr><th>Asistente</th><th>Email</th><th>Registro</th><th>Estado</th></tr></thead><tbody>
          <tr v-for="person in participants" :key="person.id"><td>{{ person.userName }} {{ person.userLastName }}</td><td>{{ person.userEmail }}</td><td>{{ formatDate(person.joinedAt) }}</td><td><v-chip size="small" :color="person.isPresent ? 'success' : 'warning'">{{ person.isPresent ? 'Presente' : 'Ausente' }}</v-chip></td></tr>
        </tbody></v-table>
        <div v-if="!participants.length" class="empty-state">Ningún asistente ha registrado su presencia.</div>
      </v-card-text></v-card>
    </v-dialog>

    <v-dialog v-model="accessDialog" max-width="850">
      <v-card class="panel"><DialogHeader icon="mdi-key-variant" @close="accessDialog = false">Accesos de votantes</DialogHeader><v-card-text>
        <div class="d-flex flex-wrap ga-2 mb-4">
          <v-btn variant="tonal" prepend-icon="mdi-content-copy" :disabled="!accesses.length" @click="copyAllAccesses">Copiar todos</v-btn>
          <v-btn color="primary" prepend-icon="mdi-refresh" :loading="saving" @click="generateAccesses">Sincronizar / generar faltantes</v-btn>
        </div>
        <v-list bg-color="transparent">
          <v-list-item v-for="access in accesses" :key="access.id" :title="`${access.userName} (${access.userEmail})`" :subtitle="access.accessUrl" class="break-word">
            <template #prepend><v-chip size="x-small" :color="access.isRevoked ? 'error' : 'success'">{{ access.isRevoked ? 'Revocado' : 'Activo' }}</v-chip></template>
            <template #append>
              <v-btn icon="mdi-content-copy" variant="text" @click="copyAccess(access)" />
              <v-btn icon="mdi-refresh" variant="text" @click="regenerateAccess(access)" />
              <v-btn v-if="!access.isRevoked" icon="mdi-cancel" color="error" variant="text" @click="revokeAccess(access)" />
            </template>
          </v-list-item>
        </v-list>
        <div v-if="!accesses.length" class="empty-state">No hay accesos generados.</div>
      </v-card-text></v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { computed, h, onBeforeUnmount, onMounted, reactive, ref, resolveComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { authStore } from '@/stores/auth'
import { apiMessage, services, VOTING_HUB_URL } from '@/services/api'
import { MajorityType, QuorumType, VotingState } from '@/constants'
import { useConfirmDialog } from '@/composables/useConfirmDialog'
import { useToasts } from '@/composables/useToasts'
import DialogHeader from '@/components/DialogHeader.vue'

const DialogActions = {
  props: { loading: Boolean },
  emits: ['cancel'],
  setup(props, { emit }) {
    return () => h('div', { class: 'd-flex justify-end ga-2 mt-2' }, [
      h(resolveComponent('VBtn'), { variant: 'text', onClick: () => emit('cancel') }, () => 'Cancelar'),
      h(resolveComponent('VBtn'), { color: 'primary', type: 'submit', loading: props.loading }, () => 'Guardar')
    ])
  }
}
const route = useRoute()
const router = useRouter()
const toast = useToasts()
const { confirmAction } = useConfirmDialog()
const id = computed(() => route.params.id)
const meeting = ref(null)
const quorum = ref(null)
const statuses = ref({})
const participants = ref([])
const accesses = ref([])
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const present = ref(false)
const agendaDialog = ref(false)
const proposalDialog = ref(false)
const uploadDialog = ref(false)
const meetingDialog = ref(false)
const settingsDialog = ref(false)
const participantsDialog = ref(false)
const accessDialog = ref(false)
const editingProposal = ref(null)
const uploadProposalId = ref('')
const agendaForm = reactive({ title: '', description: '', order: 1 })
const proposalForm = reactive({ agendaItemId: '', title: '', description: '', options: ['A favor', 'En contra', 'Abstención'] })
const uploadForm = reactive({ file: [], title: '', description: '' })
const meetingForm = reactive({})
const settingsForm = reactive({})
const agendaItems = computed(() => (meeting.value?.agendaItems || []).map((item) => ({ title: item.title, value: item.id })))
const breadcrumbs = computed(() => [{ title: 'Comunidades', to: '/communities' }, { title: meeting.value?.communityName, to: `/communities/${meeting.value?.communityId}` }, { title: meeting.value?.title }])
const attendancePercentage = computed(() => quorum.value?.eligibleMembers ? quorum.value.presentMembers / quorum.value.eligibleMembers * 100 : 0)
const meetingTypes = [{ title: 'Ordinaria', value: 0 }, { title: 'Extraordinaria', value: 1 }]
const majorityTypes = [{ title: 'Mayoría simple', value: 1 }, { title: 'Mayoría de votos emitidos', value: 2 }, { title: 'Mayoría cualificada', value: 3 }]
const abstentionPolicies = [{ title: 'Excluidas', value: 1 }, { title: 'Incluidas en el denominador', value: 2 }, { title: 'Contadas en contra', value: 3 }]
let connection

onMounted(async () => {
  await load()
  connect()
})
onBeforeUnmount(() => connection?.stop())
async function load() {
  loading.value = true
  try {
    ;[meeting.value, statuses.value, quorum.value] = await Promise.all([services.meeting(id.value), services.meetingVotingStatuses(id.value), services.quorum(id.value)])
    agendaForm.order = Math.max(0, ...(meeting.value.agendaItems || []).map((item) => item.order || 0)) + 1
  } catch (err) { error.value = apiMessage(err, 'Error cargando la reunión.') }
  finally { loading.value = false }
}
function connect() {
  if (!authStore.token) return
  connection = new HubConnectionBuilder().withUrl(`${VOTING_HUB_URL}?access_token=${authStore.token}`).withAutomaticReconnect().build()
  const join = () => connection.invoke('JoinMeetingGroup', id.value).catch(() => {})
  for (const event of ['OnSessionCreated', 'OnSessionPrepared', 'OnSessionOpened', 'OnSessionClosed', 'OnVoteCast']) connection.on(event, async () => { statuses.value = await services.meetingVotingStatuses(id.value) })
  connection.onreconnected(join)
  connection.start().then(join).catch(() => {})
}
function status(proposal) { return statuses.value[proposal.id] || {} }
async function recordAttendance() {
  try {
    await services.attendance(id.value, { userId: authStore.user.id, isPresent: true })
    present.value = true
    quorum.value = await services.quorum(id.value)
    toast.success('Asistencia registrada.')
  } catch (err) { toast.error(apiMessage(err, 'Error registrando asistencia.')) }
}
async function createAgenda() {
  await runSave(async () => {
    await services.createAgendaItem({ meetingId: id.value, ...agendaForm, description: agendaForm.description.trim() || null })
    agendaDialog.value = false
    Object.assign(agendaForm, { title: '', description: '', order: agendaForm.order + 1 })
    await load()
  }, 'Punto creado.', 'Error creando el punto.')
}
async function deleteAgenda(item) {
  if (!await confirmAction({ title: 'Eliminar punto', message: 'Se eliminarán también sus propuestas asociadas.' })) return
  try { await services.deleteAgendaItem(item.id); await load(); toast.success('Punto eliminado.') }
  catch (err) { toast.error(apiMessage(err, 'Error eliminando el punto.')) }
}
function openProposal(agendaItemId = '', proposal = null) {
  editingProposal.value = proposal
  Object.assign(proposalForm, {
    agendaItemId: agendaItemId || meeting.value.agendaItems?.[0]?.id || '',
    title: proposal?.title || '',
    description: proposal?.description || '',
    options: proposal?.options?.map((option) => option.label) || ['A favor', 'En contra', 'Abstención']
  })
  proposalDialog.value = true
}
async function saveProposal() {
  const options = proposalForm.options.map((option) => option.trim()).filter(Boolean)
  if (options.length < 2) { toast.warning('Debes indicar al menos dos opciones.'); return }
  await runSave(async () => {
    if (editingProposal.value) {
      await services.updateProposal(editingProposal.value.id, {
        title: proposalForm.title.trim(), description: proposalForm.description.trim() || null,
        order: editingProposal.value.order || 1, majorityType: editingProposal.value.majorityType,
        majorityPercentage: editingProposal.value.majorityPercentage, options
      })
    } else {
      await services.createProposal({ meetingId: id.value, agendaItemId: proposalForm.agendaItemId, title: proposalForm.title.trim(), description: proposalForm.description.trim() || null, options })
    }
    proposalDialog.value = false
    await load()
  }, 'Propuesta guardada.', 'Error guardando la propuesta.')
}
async function startVoting(proposal) {
  const existing = status(proposal)
  if (existing.sessionId) { await router.push(`/voting/${existing.sessionId}`); return }
  if (new Date() < new Date(meeting.value.scheduledAt)) { toast.warning(`No se puede iniciar antes de ${formatDate(meeting.value.scheduledAt)}.`); return }
  try {
    const session = await services.createVotingSession({
      proposalId: proposal.id, meetingId: meeting.value.id, communityId: meeting.value.communityId,
      agendaItemId: proposal.agendaItemId, title: proposal.title, description: proposal.description,
      displayOrder: proposal.order, meetingName: meeting.value.title,
      options: proposal.options.map((option) => ({ id: option.id, label: option.label }))
    })
    await services.prepareVotingSession(session.id)
    await router.push(`/voting/${session.id}`)
  } catch (err) {
    const session = err.response?.data?.session
    if (session?.id) await router.push(`/voting/${session.id}`)
    else toast.error(apiMessage(err, 'Error iniciando la sala.'))
  }
}
function openUpload(proposalId) {
  uploadProposalId.value = proposalId
  Object.assign(uploadForm, { file: [], title: '', description: '' })
  uploadDialog.value = true
}
async function uploadDocument() {
  const file = Array.isArray(uploadForm.file) ? uploadForm.file[0] : uploadForm.file
  if (!file) { toast.warning('Selecciona un archivo.'); return }
  const data = new FormData()
  data.append('file', file)
  data.append('title', uploadForm.title.trim())
  if (uploadForm.description.trim()) data.append('description', uploadForm.description.trim())
  await runSave(async () => { await services.uploadDocument(uploadProposalId.value, data); uploadDialog.value = false; await load() }, 'Documento adjuntado.', 'Error subiendo el documento.')
}
async function download(doc) {
  try {
    const blob = await services.downloadDocument(doc.id)
    const url = URL.createObjectURL(new Blob([blob]))
    const link = document.createElement('a')
    link.href = url
    link.download = doc.fileName || 'documento'
    link.click()
    URL.revokeObjectURL(url)
  } catch (err) { toast.error(apiMessage(err, 'Error descargando el documento.')) }
}
async function deleteDocument(doc) {
  if (!await confirmAction({ title: 'Eliminar documento', message: `¿Eliminar "${doc.title}"?` })) return
  try { await services.deleteDocument(doc.id); await load(); toast.success('Documento eliminado.') }
  catch (err) { toast.error(apiMessage(err, 'Error eliminando el documento.')) }
}
function openMeetingEdit() {
  Object.assign(meetingForm, {
    title: meeting.value.title, type: meeting.value.type, location: meeting.value.location,
    scheduledAt: toInputDate(meeting.value.scheduledAt), secondCallAt: toInputDate(meeting.value.secondCallAt),
    isTransparent: meeting.value.isTransparent
  })
  meetingDialog.value = true
}
async function saveMeeting() {
  await runSave(async () => {
    meeting.value = await services.updateMeeting(id.value, {
      ...meetingForm, type: Number(meetingForm.type), scheduledAt: new Date(meetingForm.scheduledAt).toISOString(),
      secondCallAt: meetingForm.secondCallAt ? new Date(meetingForm.secondCallAt).toISOString() : null
    })
    meetingDialog.value = false
  }, 'Reunión actualizada.', 'Error actualizando la reunión.')
}
function openSettings() {
  Object.assign(settingsForm, meeting.value.votingSettings || { quorumEnabled: true, quorumPercentage: 50, requireQuorumForVoting: true, defaultMajorityType: 1, defaultMajorityPercentage: 66.67, abstentionPolicy: 1 })
  settingsDialog.value = true
}
async function saveSettings() {
  await runSave(async () => {
    await services.saveMeetingVotingSettings(id.value, {
      ...settingsForm, quorumType: QuorumType.PercentageOfEligibleMembers,
      defaultMajorityType: Number(settingsForm.defaultMajorityType),
      abstentionPolicy: Number(settingsForm.abstentionPolicy),
      defaultMajorityPercentage: settingsForm.defaultMajorityType === MajorityType.QualifiedMajority ? Number(settingsForm.defaultMajorityPercentage) : undefined
    })
    settingsDialog.value = false
    await load()
  }, 'Reglas actualizadas.', 'Error guardando las reglas.')
}
async function openParticipants() {
  participantsDialog.value = true
  try { participants.value = await services.participants(id.value) }
  catch (err) { toast.error(apiMessage(err, 'Error cargando asistentes.')) }
}
async function openAccesses() {
  accessDialog.value = true
  try { accesses.value = await services.voterAccesses(id.value) }
  catch (err) { toast.error(apiMessage(err, 'Error cargando accesos.')) }
}
async function generateAccesses() {
  await runSave(async () => { accesses.value = await services.generateVoterAccesses(id.value) }, 'Accesos sincronizados.', 'Error generando accesos.')
}
async function regenerateAccess(access) {
  try {
    const updated = await services.regenerateVoterAccess(id.value, access.userId)
    accesses.value = accesses.value.map((item) => item.userId === access.userId ? updated : item)
    toast.success('Acceso regenerado.')
  } catch (err) { toast.error(apiMessage(err, 'Error regenerando el acceso.')) }
}
async function revokeAccess(access) {
  if (!await confirmAction({ title: 'Revocar acceso', message: `El acceso de ${access.userName} dejará de funcionar.` })) return
  try { await services.revokeVoterAccess(id.value, access.userId); accesses.value = await services.voterAccesses(id.value); toast.success('Acceso revocado.') }
  catch (err) { toast.error(apiMessage(err, 'Error revocando el acceso.')) }
}
async function copyAccess(access) {
  const code = access.code && access.code !== '[PROTECTED_CODE]' ? access.code : 'Código registrado'
  await navigator.clipboard.writeText(`Acceso a "${meeting.value.title}":\nEnlace: ${access.accessUrl}\nCódigo: ${code}`)
  toast.success('Acceso copiado.')
}
async function copyAllAccesses() {
  await navigator.clipboard.writeText(accesses.value.map((access) => `${access.userName} (${access.userEmail})\nEnlace: ${access.accessUrl}\nCódigo: ${access.code || '(registrado)'}`).join('\n\n'))
  toast.success('Todos los accesos copiados.')
}
async function runSave(action, success, fallback) {
  saving.value = true
  try { await action(); toast.success(success) }
  catch (err) { toast.error(apiMessage(err, fallback)) }
  finally { saving.value = false }
}
function formatDate(value) { return value ? new Date(value).toLocaleString('es-ES') : 'No especificado' }
function toInputDate(value) {
  if (!value) return ''
  const date = new Date(value)
  const offset = date.getTimezoneOffset() * 60000
  return new Date(date.getTime() - offset).toISOString().slice(0, 16)
}
</script>
