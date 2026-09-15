<template>
  <div v-if="loading" class="loading-page"><v-progress-circular indeterminate color="primary" /><span>Verificando credenciales de votación...</span></div>
  <v-alert v-else-if="error || !session" type="error" variant="tonal">{{ error || 'Sesión no encontrada.' }}</v-alert>
  <div v-else>
    <v-breadcrumbs :items="breadcrumbs" class="px-0" />
    <v-alert color="primary" variant="tonal" class="mb-5" icon="mdi-shield-lock-outline">
      <div class="d-flex flex-wrap align-center ga-2">
        <div><strong>Votación privada y verificada</strong><div class="text-body-2">{{ session.meetingName }} · {{ session.title }}</div></div>
        <v-spacer />
        <v-chip size="small" :color="connected ? 'success' : 'warning'">{{ connected ? 'Conexión en tiempo real' : 'Reconectando…' }}</v-chip>
      </div>
    </v-alert>

    <v-card class="panel mb-5">
      <v-card-text class="pa-6">
        <div class="page-header mb-0">
          <div>
            <v-chip :color="isOpen ? 'success' : isClosed ? 'warning' : 'info'" size="small">{{ isOpen ? 'Votación activa' : isClosed ? 'Votación finalizada' : 'En espera de apertura' }}</v-chip>
            <h1 class="page-title mt-3">{{ session.title }}</h1>
            <p v-if="session.description" class="page-subtitle">{{ session.description }}</p>
          </div>
          <div v-if="authStore.isAdmin">
            <v-btn v-if="!isOpen && !isClosed" color="success" prepend-icon="mdi-play" @click="openSession">Iniciar votación</v-btn>
            <v-btn v-else-if="isOpen" color="error" prepend-icon="mdi-lock" @click="closeSession">Cerrar y calcular resultado</v-btn>
          </div>
        </div>
      </v-card-text>
    </v-card>

    <v-alert v-if="message" type="success" variant="tonal" closable class="mb-5" @click:close="message = ''">{{ message }}</v-alert>

    <v-card class="panel mb-5">
      <v-card-text>
        <div class="responsive-grid">
          <div><div class="text-caption text-primary font-weight-bold">REGLA DE MAYORÍA</div><div class="text-h6">{{ majority.title }}</div><div class="muted">{{ majority.description }}</div></div>
          <div>
            <div class="d-flex justify-space-between mb-2"><span>Participación</span><strong>{{ live.totalVotesCast || 0 }} de {{ live.totalBallots || 0 }} ({{ Number(live.participationPercentage || 0).toFixed(1) }}%)</strong></div>
            <v-progress-linear :model-value="live.participationPercentage || 0" color="primary" height="10" rounded />
          </div>
        </div>
      </v-card-text>
    </v-card>

    <div class="responsive-grid">
      <v-card class="panel">
        <v-card-title><v-icon icon="mdi-vote-outline" color="primary" class="mr-2" />Opciones de voto</v-card-title>
        <v-card-text>
          <v-alert v-if="!isOpen && !isClosed" type="warning" variant="tonal">Permanece en esta pantalla. Podrás votar cuando el administrador abra la sesión.</v-alert>
          <v-alert v-else-if="isClosed" type="info" variant="tonal">La votación ha finalizado. Consulta el resultado oficial.</v-alert>
          <template v-else>
            <v-card
              v-for="option in session.options"
              :key="option.id"
              class="option-card mb-3"
              :class="{ selected: selectedOptionId === option.id }"
              :disabled="Boolean(confirmedOptionId)"
              variant="outlined"
              tabindex="0"
              role="radio"
              :aria-checked="selectedOptionId === option.id"
              @click="select(option.id)"
              @keydown.enter="select(option.id)"
            >
              <v-card-text class="d-flex align-center">
                <v-icon :icon="selectedOptionId === option.id ? 'mdi-radiobox-marked' : 'mdi-radiobox-blank'" color="primary" class="mr-3" />
                <strong>{{ option.label }}</strong>
                <v-spacer />
                <v-icon v-if="confirmedOptionId === option.id" icon="mdi-check-decagram" color="success" />
              </v-card-text>
            </v-card>
            <v-alert v-if="selectedOptionId && !confirmedOptionId" type="info" variant="tonal" class="mt-4">
              Has seleccionado <strong>{{ selectedLabel }}</strong>. Confirma tu elección para emitir el voto definitivamente.
              <div class="mt-3"><v-btn block color="primary" prepend-icon="mdi-check-decagram" @click="confirmDialog = true">Confirmar voto definitivo</v-btn></div>
            </v-alert>
            <v-alert v-if="confirmedOptionId" type="success" variant="tonal">Tu voto por <strong>{{ selectedLabel }}</strong> ha sido confirmado.</v-alert>
          </template>
        </v-card-text>
      </v-card>

      <v-card class="panel">
        <v-card-title><v-icon icon="mdi-chart-bar" color="primary" class="mr-2" />{{ isClosed ? 'Resultado oficial auditado' : 'Escrutinio en tiempo real' }}</v-card-title>
        <v-card-text>
          <v-alert v-if="result" :type="result.approved ? 'success' : 'error'" variant="tonal" class="mb-5 text-center">
            <div class="text-h6">{{ result.approved ? 'Propuesta aprobada' : 'Propuesta rechazada' }}</div>
            <div class="text-h5 font-weight-bold mt-2">{{ result.winningOptionLabel }}</div>
            <div class="text-caption mt-2">Cerrado por {{ result.closedByUserName }} · {{ result.totalVotesCast }} votos emitidos</div>
          </v-alert>
          <div v-for="option in session.options" :key="option.id" class="result-row">
            <div class="d-flex justify-space-between mb-1"><span>{{ option.label }}</span><strong>{{ votesFor(option.id) }} votos ({{ percentFor(option.id).toFixed(1) }}%)</strong></div>
            <v-progress-linear :model-value="percentFor(option.id)" color="primary" height="9" rounded />
          </div>
        </v-card-text>
      </v-card>
    </div>

    <v-dialog v-model="confirmDialog" max-width="460">
      <v-card class="panel">
        <DialogHeader icon="mdi-alert-outline" @close="confirmDialog = false">¿Confirmar voto?</DialogHeader>
        <v-card-text>Vas a emitir tu voto por <strong>{{ selectedLabel }}</strong>. Esta acción no puede deshacerse.</v-card-text>
        <v-card-actions><v-spacer /><v-btn variant="text" @click="confirmDialog = false">Modificar</v-btn><v-btn color="primary" :loading="voting" @click="castVote">Sí, emitir voto</v-btn></v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { authStore } from '@/stores/auth'
import { apiMessage, services, VOTING_HUB_URL } from '@/services/api'
import { VotingState } from '@/constants'
import { useConfirmDialog } from '@/composables/useConfirmDialog'
import { useToasts } from '@/composables/useToasts'
import DialogHeader from '@/components/DialogHeader.vue'

const route = useRoute()
const toast = useToasts()
const { confirmAction } = useConfirmDialog()
const sessionId = computed(() => route.params.sessionId)
const session = ref(null)
const result = ref(null)
const loading = ref(true)
const voting = ref(false)
const connected = ref(false)
const error = ref('')
const message = ref('')
const selectedOptionId = ref('')
const confirmedOptionId = ref('')
const confirmDialog = ref(false)
const isOpen = computed(() => session.value?.state === VotingState.Open)
const isClosed = computed(() => session.value?.state === VotingState.Closed)
const live = computed(() => session.value?.liveStats || {})
const selectedLabel = computed(() => session.value?.options.find((option) => option.id === selectedOptionId.value)?.label || '')
const breadcrumbs = computed(() => [{ title: 'Comunidades', to: '/communities' }, { title: 'Volver a la reunión', to: `/meetings/${session.value?.meetingId}` }, { title: 'Votación privada' }])
const majority = computed(() => {
  if (session.value?.majorityType === 2) return { title: 'Mayoría absoluta', description: 'Requiere más del 50% de los votos emitidos.' }
  if (session.value?.majorityType === 3) return { title: `Mayoría cualificada (${session.value.majorityPercentage || 66.67}%)`, description: 'Requiere alcanzar el porcentaje cualificado.' }
  return { title: 'Mayoría simple', description: 'Requiere más votos a favor que en contra.' }
})
let connection

onMounted(async () => {
  await fetchSession()
  connect()
})
onBeforeUnmount(() => connection?.stop())

async function fetchSession() {
  try {
    session.value = await services.votingSession(sessionId.value)
    const ballot = session.value.ballots?.find((item) => item.userId === authStore.user?.id && item.status === 1 && item.vote)
    if (ballot) selectedOptionId.value = confirmedOptionId.value = ballot.vote.selectedOptionId
    if (session.value.state === VotingState.Closed) await fetchResult(session.value.proposalId)
  } catch (err) { error.value = apiMessage(err, 'Error cargando la sesión.') }
  finally { loading.value = false }
}
function connect() {
  if (!authStore.token || !session.value) return
  connection = new HubConnectionBuilder().withUrl(`${VOTING_HUB_URL}?access_token=${authStore.token}`).withAutomaticReconnect().build()
  const join = async () => {
    await connection.invoke('JoinMeetingGroup', session.value.meetingId).catch(() => {})
    await connection.invoke('JoinSessionGroup', session.value.id).catch(() => {})
  }
  connection.onreconnecting(() => { connected.value = false })
  connection.onreconnected(() => { connected.value = true; join() })
  connection.on('OnSessionOpened', async () => { message.value = 'La votación ha sido abierta.'; await fetchSession() })
  connection.on('OnVoteCast', (data) => { if (data?.liveStats) session.value.liveStats = data.liveStats })
  connection.on('OnSessionClosed', async (data) => { message.value = 'La votación ha concluido.'; await fetchSession(); if (data?.resultId) await fetchResult(data.resultId) })
  connection.start().then(() => { connected.value = true; join() }).catch(() => {})
}
async function fetchResult(id) {
  try { result.value = await services.votingResult(id) }
  catch { result.value = null }
}
async function openSession() {
  try { await services.openVotingSession(sessionId.value); await fetchSession() }
  catch (err) { toast.error(apiMessage(err, 'Error abriendo la votación.')) }
}
async function closeSession() {
  if (!await confirmAction({ title: 'Cerrar votación', message: 'Se calculará el resultado oficial y no se admitirán más votos.' })) return
  try { await services.closeVotingSession(sessionId.value); await fetchSession() }
  catch (err) { toast.error(apiMessage(err, 'Error cerrando la votación.')) }
}
function select(id) {
  if (!confirmedOptionId.value) selectedOptionId.value = id
}
async function castVote() {
  if (!selectedOptionId.value) return
  voting.value = true
  try {
    await services.castVote(sessionId.value, selectedOptionId.value)
    confirmedOptionId.value = selectedOptionId.value
    confirmDialog.value = false
    message.value = '¡Voto emitido y verificado!'
    await fetchSession()
  } catch (err) { toast.error(apiMessage(err, 'Error al emitir el voto.')) }
  finally { voting.value = false }
}
function votesFor(id) { return Number((result.value?.votesByOption || live.value.votesByOption || {})[id] || 0) }
function percentFor(id) {
  const total = Number(result.value?.totalVotesCast ?? live.value.totalVotesCast ?? 0)
  return total ? votesFor(id) / total * 100 : 0
}
</script>
