import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { apiClient, votingApiClient, API_BASE_URL, VOTING_HUB_URL } from '../api/client';
import { useAuth } from '../contexts/AuthContext';
import { Meeting, VotingState, AgendaItem, QuorumStatus, VotingSettings } from '../types';
import { CreateAgendaItemModal } from '../components/CreateAgendaItemModal';
import { CreateProposalModal } from '../components/CreateProposalModal';
import { UploadDocumentModal } from '../components/UploadDocumentModal';
import { QuorumCard } from '../components/QuorumCard';
import { VotingSettingsModal } from '../components/VotingSettingsModal';
import { EditMeetingModal } from '../components/EditMeetingModal';
import { AttendanceListModal } from '../components/AttendanceListModal';
import {
  Calendar,
  MapPin,
  Clock,
  Plus,
  FileText,
  Vote,
  Play,
  CheckCircle2,
  ChevronRight,
  Upload,
  Download,
  Trash2,
  ListOrdered,
  Lock,
  ShieldCheck,
  Edit3,
  UserCheck,
} from 'lucide-react';

export const MeetingDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { user, isAdmin, token } = useAuth();
  const navigate = useNavigate();

  const [meeting, setMeeting] = useState<Meeting | null>(null);
  const [loading, setLoading] = useState(true);
  const [quorumStatus, setQuorumStatus] = useState<QuorumStatus | null>(null);
  const [isPresent, setIsPresent] = useState(false);
  const [votingStatuses, setVotingStatuses] = useState<Record<string, { state: number; sessionId?: string; resultId?: string }>>({});

  // Modals
  const [isAgendaItemModalOpen, setIsAgendaItemModalOpen] = useState(false);
  const [isProposalModalOpen, setIsProposalModalOpen] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [isEditMeetingOpen, setIsEditMeetingOpen] = useState(false);
  const [isAttendanceListOpen, setIsAttendanceListOpen] = useState(false);
  const [selectedAgendaItemId, setSelectedAgendaItemId] = useState<string | undefined>(undefined);
  const [uploadDocProposalId, setUploadDocProposalId] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      fetchMeeting();
      fetchVotingStatuses();
      fetchQuorumStatus();
    }
  }, [id]);

  const fetchQuorumStatus = async () => {
    try {
      const res = await apiClient.get<QuorumStatus>(`/meetings/${id}/quorum-status`);
      setQuorumStatus(res.data);
    } catch (err) {
      console.error('Error fetching quorum status', err);
    }
  };

  const handleRecordAttendance = async () => {
    if (!id || !user) return;
    try {
      await apiClient.post(`/meetings/${id}/attendance`, {
        userId: user.id,
        isPresent: true,
      });
      setIsPresent(true);
      fetchQuorumStatus();
    } catch (err) {
      alert('Error registrando asistencia');
    }
  };

  const handleSaveSettings = async (updated: Partial<VotingSettings>) => {
    if (!id) return;
    await apiClient.put(`/meetings/${id}/voting-settings`, updated);
    fetchMeeting();
    fetchQuorumStatus();
  };

  // Connect to SignalR Hub for real-time status updates
  useEffect(() => {
    if (!id || !token) return;

    const newConnection = new HubConnectionBuilder()
      .withUrl(`${VOTING_HUB_URL}?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    const joinGroup = () => {
      newConnection.invoke('JoinMeetingGroup', id).catch(err => console.error('SignalR JoinMeetingGroup error', err));
    };

    newConnection.onreconnected(joinGroup);

    newConnection.start()
      .then(joinGroup)
      .catch(err => console.error('SignalR connection error in MeetingDetail', err));

    const refreshStatuses = () => {
      fetchVotingStatuses();
    };

    newConnection.on('OnSessionCreated', refreshStatuses);
    newConnection.on('OnSessionPrepared', refreshStatuses);
    newConnection.on('OnSessionOpened', refreshStatuses);
    newConnection.on('OnSessionClosed', refreshStatuses);
    newConnection.on('OnVoteCast', refreshStatuses);

    return () => {
      newConnection.stop();
    };
  }, [id, token]);

  const fetchMeeting = async () => {
    try {
      const res = await apiClient.get<Meeting>(`/meetings/${id}`);
      setMeeting(res.data);
    } catch (err) {
      console.error('Error fetching meeting details', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchVotingStatuses = async () => {
    try {
      const res = await votingApiClient.get<Record<string, { state: number; sessionId?: string; resultId?: string }>>(`/meetings/${id}/statuses`);
      setVotingStatuses(res.data);
    } catch (err) {
      console.error('Error fetching voting statuses', err);
    }
  };

  const handleStartVoting = async (proposal: any) => {
    if (!meeting) return;
    try {
      const createRes = await votingApiClient.post('/sessions', {
        proposalId: proposal.id,
        meetingId: meeting.id,
        communityId: meeting.communityId,
        agendaItemId: proposal.agendaItemId,
        title: proposal.title,
        description: proposal.description,
        displayOrder: proposal.order,
        meetingName: meeting.title,
        options: proposal.options.map((o: any) => ({ id: o.id, label: o.label })),
      });

      const session = createRes.data;
      await votingApiClient.post(`/sessions/${session.id}/prepare`);
      navigate(`/voting/${session.id}`);
    } catch (err: any) {
      if (err.response?.data?.session) {
        const session = err.response.data.session;
        navigate(`/voting/${session.id}`);
      } else {
        alert(err.response?.data?.error || 'Error iniciando sala de votación');
      }
    }
  };

  const handleDownloadDoc = (docId: string) => {
    window.open(`${API_BASE_URL}/documents/${docId}/download`, '_blank');
  };

  const handleDeleteDoc = async (docId: string) => {
    if (!window.confirm('¿Deseas eliminar este documento adjunto?')) return;
    try {
      await apiClient.delete(`/documents/${docId}`);
      fetchMeeting();
    } catch (err) {
      alert('Error eliminando documento');
    }
  };

  const handleDeleteAgendaItem = async (agendaItemId: string) => {
    if (!window.confirm('¿Deseas eliminar este punto del orden del día y sus propuestas asociadas?')) return;
    try {
      await apiClient.delete(`/agenda-items/${agendaItemId}`);
      fetchMeeting();
    } catch (err) {
      alert('Error eliminando punto del día');
    }
  };

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-secondary)' }}>Cargando datos de la reunión...</div>;
  }

  if (!meeting) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--danger)' }}>Reunión no encontrada.</div>;
  }

  return (
    <div>
      {/* Breadcrumb */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
        <Link to="/communities" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>Comunidades</Link>
        <ChevronRight size={14} />
        <Link to={`/communities/${meeting.communityId}`} style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>{meeting.communityName}</Link>
        <ChevronRight size={14} />
        <span style={{ color: 'var(--text-primary)', fontWeight: '600' }}>{meeting.title}</span>
      </div>

      {/* Header Banner */}
      <div className="glass-panel" style={{ padding: '28px', marginBottom: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px' }}>
          <div>
            <span className="badge badge-privacy" style={{ marginBottom: '8px' }}>
              {meeting.type === 0 ? 'Junta Ordinaria' : 'Junta Extraordinaria'}
            </span>
            <h1 style={{ fontSize: '1.8rem', marginTop: '6px' }}>{meeting.title}</h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.92rem', marginTop: '4px' }}>
              Comunidad: <strong style={{ color: 'var(--text-primary)' }}>{meeting.communityName}</strong>
            </p>
          </div>

          {isAdmin && (
            <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
              <button
                onClick={() => setIsEditMeetingOpen(true)}
                className="btn btn-secondary"
              >
                <Edit3 size={16} /> Editar Datos / Fechas
              </button>
              <button
                onClick={() => setIsAgendaItemModalOpen(true)}
                className="btn btn-secondary"
              >
                <ListOrdered size={16} /> Añadir Punto
              </button>
              <button
                onClick={() => {
                  setSelectedAgendaItemId(undefined);
                  setIsProposalModalOpen(true);
                }}
                className="btn btn-primary"
              >
                <Plus size={16} /> Nueva Propuesta
              </button>
            </div>
          )}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', marginTop: '24px', paddingTop: '20px', borderTop: '1px solid var(--border-glass)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Calendar size={18} color="var(--accent)" />
            <div>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', display: 'block' }}>Fecha Convocatoria</span>
              <span style={{ fontSize: '0.9rem', fontWeight: '600' }}>{new Date(meeting.scheduledAt).toLocaleString()}</span>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <MapPin size={18} color="var(--accent)" />
            <div>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', display: 'block' }}>Ubicación / Sala</span>
              <span style={{ fontSize: '0.9rem', fontWeight: '600' }}>{meeting.location}</span>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Clock size={18} color="var(--accent)" />
            <div>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', display: 'block' }}>Cierre Votaciones</span>
              <span style={{ fontSize: '0.9rem', fontWeight: '600' }}>{new Date(meeting.votingEnd).toLocaleDateString()}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Quorum and Attendance Component */}
      <QuorumCard
        status={quorumStatus}
        isPresent={isPresent}
        onRecordAttendance={handleRecordAttendance}
        isAdmin={isAdmin}
        onOpenSettings={() => setIsSettingsModalOpen(true)}
        onOpenAttendanceList={() => setIsAttendanceListOpen(true)}
      />

      {/* Orden del Día (AgendaItems & Proposals) */}
      <h2 style={{ fontSize: '1.4rem', marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '10px' }}>
        <ListOrdered color="var(--accent)" /> Orden del Día y Propuestas ({meeting.agendaItems?.length || 0} Puntos)
      </h2>

      {(!meeting.agendaItems || meeting.agendaItems.length === 0) ? (
        <div className="glass-panel" style={{ padding: '40px', textAlign: 'center', color: 'var(--text-secondary)' }}>
          <ListOrdered size={40} style={{ opacity: 0.4, marginBottom: '12px' }} />
          <p style={{ fontSize: '1.05rem', fontWeight: '600' }}>Aún no se han añadido puntos al orden del día.</p>
          <p style={{ fontSize: '0.88rem', marginTop: '4px' }}>El administrador de la comunidad puede crear puntos del día y vincularles propuestas de votación.</p>
          {isAdmin && (
            <button onClick={() => setIsAgendaItemModalOpen(true)} className="btn btn-primary" style={{ marginTop: '20px' }}>
              <Plus size={16} /> Crear Primer Punto del Día
            </button>
          )}
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
          {meeting.agendaItems.map((item, idx) => (
            <div key={item.id} className="glass-panel" style={{ padding: '24px' }}>
              {/* AgendaItem Header */}
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '16px', flexWrap: 'wrap', gap: '12px' }}>
                <div>
                  <span className="badge badge-privacy" style={{ marginBottom: '6px' }}>
                    Punto N° {item.order || idx + 1}
                  </span>
                  <h3 style={{ fontSize: '1.25rem', marginTop: '4px' }}>{item.title}</h3>
                  {item.description && (
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginTop: '4px' }}>{item.description}</p>
                  )}
                </div>

                {isAdmin && (
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <button
                      onClick={() => {
                        setSelectedAgendaItemId(item.id);
                        setIsProposalModalOpen(true);
                      }}
                      className="btn btn-secondary btn-sm"
                    >
                      <Plus size={14} /> Nueva Propuesta en este Punto
                    </button>
                    <button
                      onClick={() => handleDeleteAgendaItem(item.id)}
                      className="btn btn-secondary btn-sm"
                      style={{ color: 'var(--danger)' }}
                      title="Eliminar punto del día"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                )}
              </div>

              {/* Proposals under this AgendaItem */}
              {(!item.proposals || item.proposals.length === 0) ? (
                <div style={{ padding: '16px', borderRadius: '12px', background: 'var(--bg-surface)', border: '1px border-glass', fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
                  Sin propuestas asignadas a este punto del orden del día.
                </div>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '16px', marginTop: '14px' }}>
                  {item.proposals.map((proposal) => {
                    const statusInfo = votingStatuses[proposal.id];
                    const isClosed = statusInfo?.state === 3;
                    const isOpen = statusInfo?.state === 2;

                    return (
                      <div
                        key={proposal.id}
                        style={{
                          padding: '20px',
                          borderRadius: '14px',
                          background: 'var(--bg-surface)',
                          border: '1px solid var(--border-glass)',
                        }}
                      >
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '12px' }}>
                          <div style={{ flex: 1 }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                              <h4 style={{ fontSize: '1.1rem' }}>{proposal.title}</h4>
                              {isClosed && (
                                <span className="badge badge-warning">
                                  <CheckCircle2 size={12} /> Votación Finalizada
                                </span>
                              )}
                              {isOpen && (
                                <span className="badge badge-success">
                                  🟢 Votación En Vivo
                                </span>
                              )}
                            </div>
                            {proposal.description && (
                              <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem', marginTop: '6px' }}>{proposal.description}</p>
                            )}
                          </div>

                          <div style={{ display: 'flex', gap: '10px' }}>
                            {isAdmin && !isClosed && (
                              <button onClick={() => handleStartVoting(proposal)} className="btn btn-primary btn-sm">
                                <Play size={14} /> {isOpen ? 'Ir a Sala En Vivo' : 'Iniciar Votación'}
                              </button>
                            )}
                            {isClosed && statusInfo?.resultId && (
                              <Link to={`/voting/${statusInfo.sessionId || proposal.id}`} className="btn btn-secondary btn-sm">
                                <Lock size={14} /> Ver Resultado Auditado
                              </Link>
                            )}
                            {!isAdmin && isOpen && (
                              <Link to={`/voting/${statusInfo?.sessionId || proposal.id}`} className="btn btn-success btn-sm">
                                <Vote size={14} /> Entrar a Votar
                              </Link>
                            )}
                          </div>
                        </div>

                        {/* Options badge list */}
                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginTop: '14px' }}>
                          <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', fontWeight: '600', alignSelf: 'center' }}>Opciones:</span>
                          {proposal.options.map((opt) => (
                            <span key={opt.id} className="badge badge-privacy" style={{ fontSize: '0.78rem' }}>
                              {opt.label}
                            </span>
                          ))}
                        </div>

                        {/* Documents section */}
                        <div style={{ marginTop: '14px', paddingTop: '12px', borderTop: '1px dashed var(--border-glass)' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                            <span style={{ fontSize: '0.8rem', fontWeight: '700', color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: '6px' }}>
                              <FileText size={14} color="var(--accent)" /> Documentos Adjuntos ({proposal.documents?.length || 0})
                            </span>
                            {isAdmin && (
                              <button
                                onClick={() => setUploadDocProposalId(proposal.id)}
                                className="btn btn-secondary btn-sm"
                                style={{ padding: '4px 8px', fontSize: '0.75rem' }}
                              >
                                <Upload size={12} /> Adjuntar Documento
                              </button>
                            )}
                          </div>

                          {proposal.documents && proposal.documents.length > 0 ? (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
                              {proposal.documents.map((doc) => (
                                <div
                                  key={doc.id}
                                  style={{
                                    display: 'flex',
                                    justifyContent: 'space-between',
                                    alignItems: 'center',
                                    padding: '8px 12px',
                                    borderRadius: '8px',
                                    background: 'rgba(255, 255, 255, 0.03)',
                                    fontSize: '0.82rem',
                                  }}
                                >
                                  <div>
                                    <strong style={{ color: 'var(--text-primary)' }}>{doc.title}</strong>
                                    {doc.description && <span style={{ color: 'var(--text-secondary)', marginLeft: '8px' }}>({doc.description})</span>}
                                    <span style={{ color: 'var(--text-secondary)', display: 'block', fontSize: '0.74rem' }}>
                                      {doc.fileName} • {(doc.fileSize / 1024).toFixed(1)} KB
                                    </span>
                                  </div>

                                  <div style={{ display: 'flex', gap: '6px' }}>
                                    <button onClick={() => handleDownloadDoc(doc.id)} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px' }} title="Descargar">
                                      <Download size={13} />
                                    </button>
                                    {isAdmin && (
                                      <button onClick={() => handleDeleteDoc(doc.id)} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px', color: 'var(--danger)' }} title="Eliminar">
                                        <Trash2 size={13} />
                                      </button>
                                    )}
                                  </div>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>
                              No hay documentos adjuntos a esta propuesta.
                            </span>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Modals */}
      <CreateAgendaItemModal
        meetingId={meeting.id}
        nextOrder={
          meeting.agendaItems && meeting.agendaItems.length > 0
            ? Math.max(...meeting.agendaItems.map(i => i.order || 0)) + 1
            : 1
        }
        isOpen={isAgendaItemModalOpen}
        onClose={() => setIsAgendaItemModalOpen(false)}
        onSuccess={() => fetchMeeting()}
      />

      <CreateProposalModal
        meetingId={meeting.id}
        agendaItems={meeting.agendaItems || []}
        defaultAgendaItemId={selectedAgendaItemId}
        isOpen={isProposalModalOpen}
        onClose={() => setIsProposalModalOpen(false)}
        onSuccess={() => fetchMeeting()}
      />

      {uploadDocProposalId && (
        <UploadDocumentModal
          proposalId={uploadDocProposalId}
          isOpen={!!uploadDocProposalId}
          onClose={() => setUploadDocProposalId(null)}
          onSuccess={() => fetchMeeting()}
        />
      )}

      <VotingSettingsModal
        isOpen={isSettingsModalOpen}
        onClose={() => setIsSettingsModalOpen(false)}
        settings={meeting.votingSettings}
        onSave={handleSaveSettings}
        title="Reglas de Quórum y Mayoría de la Reunión"
      />

      {meeting && (
        <EditMeetingModal
          meeting={meeting}
          isOpen={isEditMeetingOpen}
          onClose={() => setIsEditMeetingOpen(false)}
          onSuccess={(updated) => setMeeting(updated)}
        />
      )}

      {meeting && (
        <AttendanceListModal
          meetingId={meeting.id}
          meetingTitle={meeting.title}
          isOpen={isAttendanceListOpen}
          onClose={() => setIsAttendanceListOpen(false)}
        />
      )}
    </div>
  );
};
