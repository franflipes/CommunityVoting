import axios from 'axios'
import { authStore } from '@/stores/auth'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5004/api'
export const VOTING_API_BASE_URL = import.meta.env.VITE_VOTING_API_BASE_URL || 'http://localhost:5222/api/voting'
export const DOCUMENT_API_BASE_URL = import.meta.env.VITE_DOCUMENT_API_BASE_URL || 'http://localhost:5088/api'
export const VOTING_HUB_URL = import.meta.env.VITE_VOTING_HUB_URL || 'http://localhost:5222/hubs/voting'

export const api = axios.create({ baseURL: API_BASE_URL })
export const votingApi = axios.create({ baseURL: VOTING_API_BASE_URL })
export const documentApi = axios.create({ baseURL: DOCUMENT_API_BASE_URL })

for (const client of [api, votingApi, documentApi]) {
  client.interceptors.request.use((config) => {
    if (authStore.token) config.headers.Authorization = `Bearer ${authStore.token}`
    return config
  })
  client.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401 && authStore.token) {
        authStore.clear()
        window.dispatchEvent(new CustomEvent('auth:expired'))
      }
      return Promise.reject(error)
    }
  )
}

export function apiMessage(error, fallback = 'No se pudo completar la operación.') {
  const data = error?.response?.data
  if (typeof data === 'string' && data.trim()) return data
  if (typeof data?.error === 'string') return data.error
  if (data?.message) return data.message
  return fallback
}

export const services = {
  me: () => api.get('/auth/me').then((r) => r.data),
  login: (payload) => api.post('/auth/login', payload).then((r) => r.data),
  register: (payload) => api.post('/auth/register', payload).then((r) => r.data),
  resetPassword: (payload) => api.post('/auth/reset-password', payload).then((r) => r.data),
  meetingAccess: (payload) => api.post('/auth/meeting-access', payload).then((r) => r.data),
  verifyInvitation: (token) => api.get(`/invitations/verify/${token}`).then((r) => r.data),
  registerWithInvitation: (payload) => api.post('/auth/register-with-invitation', payload).then((r) => r.data),
  communities: () => api.get('/communities').then((r) => r.data),
  community: (id) => api.get(`/communities/${id}`).then((r) => r.data),
  createCommunity: (payload) => api.post('/communities', payload).then((r) => r.data),
  members: (id) => api.get(`/communities/${id}/members`).then((r) => r.data),
  addMember: (id, payload) => api.post(`/communities/${id}/members`, payload).then((r) => r.data),
  updateVotingRights: (id, userId, payload) => api.put(`/communities/${id}/members/${userId}/voting-rights`, payload).then((r) => r.data),
  communityVotingSettings: (id) => api.get(`/communities/${id}/voting-settings`).then((r) => r.data),
  saveCommunityVotingSettings: (id, payload) => api.put(`/communities/${id}/voting-settings`, payload).then((r) => r.data),
  invitations: (id) => api.get(`/invitations/community/${id}`).then((r) => r.data),
  createInvitation: (id, payload) => api.post(`/invitations/community/${id}`, payload).then((r) => r.data),
  meetings: (id) => api.get(`/meetings/community/${id}`).then((r) => r.data),
  meeting: (id) => api.get(`/meetings/${id}`).then((r) => r.data),
  createMeeting: (payload) => api.post('/meetings', payload).then((r) => r.data),
  updateMeeting: (id, payload) => api.put(`/meetings/${id}`, payload).then((r) => r.data),
  quorum: (id) => api.get(`/meetings/${id}/quorum-status`).then((r) => r.data),
  attendance: (id, payload) => api.post(`/meetings/${id}/attendance`, payload).then((r) => r.data),
  participants: (id) => api.get(`/meetings/${id}/participants`).then((r) => r.data),
  saveMeetingVotingSettings: (id, payload) => api.put(`/meetings/${id}/voting-settings`, payload).then((r) => r.data),
  createAgendaItem: (payload) => api.post('/agenda-items', payload).then((r) => r.data),
  deleteAgendaItem: (id) => api.delete(`/agenda-items/${id}`),
  createProposal: (payload) => api.post('/proposals', payload).then((r) => r.data),
  updateProposal: (id, payload) => api.put(`/proposals/${id}`, payload).then((r) => r.data),
  voterAccesses: (id) => api.get(`/meetings/${id}/voter-accesses`).then((r) => r.data),
  generateVoterAccesses: (id) => api.post(`/meetings/${id}/voter-accesses/generate-all`).then((r) => r.data),
  revokeVoterAccess: (id, userId) => api.post(`/meetings/${id}/voter-accesses/revoke/${userId}`),
  regenerateVoterAccess: (id, userId) => api.post(`/meetings/${id}/voter-accesses/regenerate/${userId}`).then((r) => r.data),
  uploadDocument: (proposalId, data) => documentApi.post(`/documents/proposal/${proposalId}`, data).then((r) => r.data),
  downloadDocument: (id) => documentApi.get(`/documents/${id}/download`, { responseType: 'blob' }).then((r) => r.data),
  deleteDocument: (id) => documentApi.delete(`/documents/${id}`),
  meetingVotingStatuses: (id) => votingApi.get(`/meetings/${id}/statuses`).then((r) => r.data),
  createVotingSession: (payload) => votingApi.post('/sessions', payload).then((r) => r.data),
  prepareVotingSession: (id) => votingApi.post(`/sessions/${id}/prepare`).then((r) => r.data),
  votingSession: (id) => votingApi.get(`/sessions/${id}`).then((r) => r.data),
  openVotingSession: (id) => votingApi.post(`/sessions/${id}/open`).then((r) => r.data),
  closeVotingSession: (id) => votingApi.post(`/sessions/${id}/close`).then((r) => r.data),
  castVote: (id, optionId) => votingApi.post(`/sessions/${id}/vote`, { optionId }).then((r) => r.data),
  votingResult: (id) => votingApi.get(`/results/${id}`).then((r) => r.data)
}
