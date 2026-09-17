<template>
  <div>
    <header class="page-header">
      <div>
        <h1 class="page-title">Configuración de la comunidad</h1>
        <p class="page-subtitle">Gestiona reglas de quórum, mayorías, censo e invitaciones.</p>
      </div>
      <v-select v-model="selectedId" :items="communityItems" label="Comunidad" hide-details style="max-width: 300px" />
    </header>

    <v-alert v-if="message" type="success" variant="tonal" closable class="mb-4" @click:close="message = ''">{{ message }}</v-alert>
    <div v-if="loading" class="loading-page"><v-progress-circular indeterminate color="primary" /></div>
    <v-alert v-else-if="error" type="error" variant="tonal">{{ error }}</v-alert>
    <v-alert v-else-if="!community" type="info" variant="tonal">No hay comunidades disponibles para configurar.</v-alert>
    <template v-else-if="community">
      <v-tabs v-model="tab" color="primary" class="mb-5">
        <v-tab value="voting">Reglas de votación</v-tab>
        <v-tab value="members">Censo ({{ members.length }})</v-tab>
        <v-tab value="general">Información</v-tab>
      </v-tabs>
      <v-window v-model="tab">
        <v-window-item value="voting">
          <v-card class="panel" max-width="760">
            <v-card-title>Reglas oficiales de {{ community.name }}</v-card-title>
            <v-card-text>
              <v-form @submit.prevent="saveVotingSettings">
                <v-switch v-model="settings.quorumEnabled" color="primary" label="Exigir quórum mínimo" />
                <v-text-field v-if="settings.quorumEnabled" v-model.number="settings.quorumPercentage" label="Porcentaje mínimo de asistencia" type="number" min="1" max="100" suffix="%" />
                <v-switch v-if="settings.quorumEnabled" v-model="settings.requireQuorumForVoting" color="primary" label="Bloquear votaciones si no se alcanza el quórum" />
                <v-divider class="my-4" />
                <v-select v-model="settings.defaultMajorityType" :items="majorityTypes" label="Tipo de mayoría" />
                <v-text-field v-if="settings.defaultMajorityType === MajorityType.QualifiedMajority" v-model.number="settings.defaultMajorityPercentage" label="Porcentaje de mayoría cualificada" type="number" min="50.01" max="100" step=".01" suffix="%" />
                <v-select v-model="settings.abstentionPolicy" :items="abstentionPolicies" label="Tratamiento de abstenciones" />
                <v-btn color="primary" type="submit" prepend-icon="mdi-content-save" :loading="saving">Guardar configuración</v-btn>
              </v-form>
            </v-card-text>
          </v-card>
        </v-window-item>

        <v-window-item value="members">
          <v-card class="panel">
            <v-card-title class="d-flex flex-wrap ga-2 align-center">
              Censo electoral
              <v-spacer />
              <v-btn variant="tonal" prepend-icon="mdi-link-variant" @click="openInvitations">Invitaciones</v-btn>
              <v-btn color="primary" prepend-icon="mdi-account-plus" @click="memberDialog = true">Añadir miembro</v-btn>
            </v-card-title>
            <v-card-text>
              <v-table>
                <thead><tr><th>Miembro</th><th>Email</th><th>Rol</th><th>Derecho a voto</th><th>Estado</th></tr></thead>
                <tbody>
                  <tr v-for="member in members" :key="member.id">
                    <td>{{ member.userName }} {{ member.userLastName }}</td>
                    <td>{{ member.userEmail }}</td>
                    <td><v-chip size="small" :color="member.memberRole === 1 ? 'success' : 'info'">{{ member.memberRole === 1 ? 'Administrador' : 'Miembro' }}</v-chip></td>
                    <td><v-switch :model-value="member.hasVotingRights" color="success" hide-details @update:model-value="toggleMember(member, 'hasVotingRights', $event)" /></td>
                    <td><v-switch :model-value="member.isActive" color="primary" hide-details @update:model-value="toggleMember(member, 'isActive', $event)" /></td>
                  </tr>
                </tbody>
              </v-table>
            </v-card-text>
          </v-card>
        </v-window-item>

        <v-window-item value="general">
          <v-card class="panel" max-width="620">
            <v-card-title>Datos de la comunidad</v-card-title>
            <v-card-text>
              <v-text-field :model-value="community.name" label="Nombre" readonly />
              <v-text-field :model-value="community.address" label="Dirección" readonly />
              <v-text-field :model-value="community.cif || 'No especificado'" label="CIF" readonly />
            </v-card-text>
          </v-card>
        </v-window-item>
      </v-window>
    </template>

    <v-dialog v-model="memberDialog" max-width="520">
      <v-card class="panel">
        <DialogHeader icon="mdi-account-plus" @close="memberDialog = false">Añadir miembro al censo</DialogHeader>
        <v-card-text>
          <v-form @submit.prevent="addMember">
            <v-text-field v-model="newMember.name" label="Nombre" required />
            <v-text-field v-model="newMember.lastName" label="Apellidos" />
            <v-text-field v-model="newMember.email" label="Correo electrónico" type="email" required />
            <v-select v-model="newMember.memberRole" :items="memberRoles" label="Rol" />
            <div class="d-flex justify-end ga-2"><v-btn variant="text" @click="memberDialog = false">Cancelar</v-btn><v-btn color="primary" type="submit" :loading="saving">Añadir</v-btn></div>
          </v-form>
        </v-card-text>
      </v-card>
    </v-dialog>

    <v-dialog v-model="invitationDialog" max-width="720">
      <v-card class="panel">
        <DialogHeader icon="mdi-link-variant" @close="invitationDialog = false">Invitaciones de {{ community?.name }}</DialogHeader>
        <v-card-text>
          <v-row dense>
            <v-col cols="12" sm="5"><v-select v-model="inviteForm.expiresInDays" :items="expirationItems" label="Caducidad" /></v-col>
            <v-col cols="12" sm="4"><v-select v-model="inviteForm.maxUses" :items="useItems" label="Límite de usos" /></v-col>
            <v-col cols="12" sm="3"><v-btn block color="primary" height="48" :loading="saving" @click="createInvitation">Generar</v-btn></v-col>
          </v-row>
          <v-list v-if="invitations.length" bg-color="transparent" class="mt-3">
            <v-list-item v-for="invite in invitations" :key="invite.id" :title="invite.isValid ? 'Enlace activo' : 'Expirado / agotado'" :subtitle="`Usos: ${invite.usesCount}${invite.maxUses ? `/${invite.maxUses}` : ' (ilimitado)'}`">
              <template #append><v-btn icon="mdi-content-copy" variant="text" @click="copy(invite.inviteUrl)" /></template>
            </v-list-item>
          </v-list>
          <div v-else class="empty-state">No hay invitaciones generadas.</div>
        </v-card-text>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { MajorityType, QuorumType, UserRole } from '@/constants'
import { apiMessage, services } from '@/services/api'
import { useToasts } from '@/composables/useToasts'
import DialogHeader from '@/components/DialogHeader.vue'

const route = useRoute()
const toast = useToasts()
const communities = ref([])
const selectedId = ref(String(route.params.id || ''))
const community = ref(null)
const members = ref([])
const loading = ref(true)
const saving = ref(false)
const error = ref('')
const message = ref('')
const tab = ref('voting')
const memberDialog = ref(false)
const invitationDialog = ref(false)
const invitations = ref([])
const defaultSettings = {
  quorumEnabled: true,
  quorumPercentage: 50,
  requireQuorumForVoting: true,
  defaultMajorityType: 1,
  defaultMajorityPercentage: 66.67,
  abstentionPolicy: 1
}
const settings = reactive({ ...defaultSettings })
const newMember = reactive({ name: '', lastName: '', email: '', memberRole: UserRole.CommunityMember })
const inviteForm = reactive({ expiresInDays: 7, maxUses: 0 })
const communityItems = computed(() => communities.value.map((c) => ({ title: c.name, value: c.id })))
const majorityTypes = [{ title: 'Mayoría simple', value: 1 }, { title: 'Mayoría de votos emitidos', value: 2 }, { title: 'Mayoría cualificada', value: 3 }]
const abstentionPolicies = [{ title: 'Excluidas del cómputo', value: 1 }, { title: 'Incluidas en el denominador', value: 2 }, { title: 'Contabilizadas en contra', value: 3 }]
const memberRoles = [{ title: 'Miembro votante', value: 2 }, { title: 'Administrador de comunidad', value: 1 }]
const expirationItems = [{ title: '7 días', value: 7 }, { title: '14 días', value: 14 }, { title: '30 días', value: 30 }, { title: 'Sin expiración', value: 0 }]
const useItems = [{ title: 'Ilimitados', value: 0 }, { title: '1 uso', value: 1 }, { title: '5 usos', value: 5 }, { title: '10 usos', value: 10 }]

onMounted(async () => {
  try {
    communities.value = await services.communities()
    if (!selectedId.value) selectedId.value = communities.value[0]?.id || ''
  } catch (err) { error.value = apiMessage(err, 'Error cargando comunidades.') }
  if (!selectedId.value) loading.value = false
})
watch(selectedId, loadCommunity, { immediate: true })
watch(() => route.params.id, (id) => {
  const routeId = String(id || '')
  if (routeId && routeId !== selectedId.value) selectedId.value = routeId
})

async function loadCommunity(id) {
  if (!id) {
    community.value = null
    members.value = []
    loading.value = false
    return
  }
  loading.value = true
  error.value = ''
  message.value = ''
  community.value = null
  members.value = []
  Object.assign(settings, defaultSettings)
  try {
    const [current, currentMembers, currentSettings] = await Promise.all([services.community(id), services.members(id), services.communityVotingSettings(id)])
    community.value = current
    members.value = currentMembers
    Object.assign(settings, defaultSettings, {
      quorumEnabled: currentSettings.quorumEnabled,
      quorumPercentage: currentSettings.quorumPercentage,
      requireQuorumForVoting: currentSettings.requireQuorumForVoting,
      defaultMajorityType: currentSettings.defaultMajorityType,
      defaultMajorityPercentage: currentSettings.defaultMajorityPercentage ?? defaultSettings.defaultMajorityPercentage,
      abstentionPolicy: currentSettings.abstentionPolicy
    })
  } catch (err) {
    error.value = apiMessage(err, 'Error cargando la configuración.')
  }
  finally { loading.value = false }
}
async function saveVotingSettings() {
  saving.value = true
  try {
    await services.saveCommunityVotingSettings(selectedId.value, {
      ...settings,
      quorumType: QuorumType.PercentageOfEligibleMembers,
      defaultMajorityType: Number(settings.defaultMajorityType),
      abstentionPolicy: Number(settings.abstentionPolicy),
      defaultMajorityPercentage: settings.defaultMajorityType === MajorityType.QualifiedMajority ? Number(settings.defaultMajorityPercentage) : undefined
    })
    message.value = 'Reglas de votación actualizadas correctamente.'
  } catch (err) { toast.error(apiMessage(err, 'Error guardando las reglas.')) }
  finally { saving.value = false }
}
async function toggleMember(member, field, value) {
  const payload = { hasVotingRights: member.hasVotingRights, isActive: member.isActive, [field]: value }
  try {
    await services.updateVotingRights(selectedId.value, member.userId, payload)
    member[field] = value
  } catch (err) { toast.error(apiMessage(err, 'Error actualizando el miembro.')) }
}
async function addMember() {
  saving.value = true
  try {
    await services.addMember(selectedId.value, { ...newMember, memberRole: Number(newMember.memberRole) })
    memberDialog.value = false
    Object.assign(newMember, { name: '', lastName: '', email: '', memberRole: 2 })
    members.value = await services.members(selectedId.value)
    toast.success('Miembro incorporado al censo.')
  } catch (err) { toast.error(apiMessage(err, 'Error añadiendo el miembro.')) }
  finally { saving.value = false }
}
async function openInvitations() {
  invitationDialog.value = true
  try { invitations.value = await services.invitations(selectedId.value) }
  catch (err) { toast.error(apiMessage(err, 'Error cargando invitaciones.')) }
}
async function createInvitation() {
  saving.value = true
  try {
    const created = await services.createInvitation(selectedId.value, { communityId: selectedId.value, ...inviteForm })
    invitations.value = [created, ...invitations.value]
    try {
      await copy(created.inviteUrl)
    } catch {
      toast.warning('Invitación creada, pero no se pudo copiar el enlace.')
    }
  } catch (err) { toast.error(apiMessage(err, 'Error generando la invitación.')) }
  finally { saving.value = false }
}
async function copy(text) {
  try {
    await navigator.clipboard.writeText(text)
    toast.success('Enlace copiado al portapapeles.')
  } catch {
    toast.error('No se pudo copiar el enlace al portapapeles.')
  }
}
</script>
