import React, { useState, useEffect, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuth } from '../contexts/AuthContext';
import { votingApiClient, VOTING_HUB_URL } from '../api/client';
import { VotingSession, VotingState, VotingResult } from '../types';
import { VotingOption } from '../components/VotingOption';
import { PrivacyHeader } from '../components/PrivacyHeader';
import { Vote, CheckCircle2, Play, Lock, ChevronRight, Award, Users, AlertTriangle, ShieldCheck } from 'lucide-react';

export const VotingRoomPage: React.FC = () => {
  const { sessionId } = useParams<{ sessionId: string }>();
  const { user, token, isAdmin } = useAuth();

  const [session, setSession] = useState<VotingSession | null>(null);
  const [result, setResult] = useState<VotingResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [votingLoading, setVotingLoading] = useState(false);

  // Double action state
  const [selectedOptionId, setSelectedOptionId] = useState<string | null>(null);
  const [confirmedOptionId, setConfirmedOptionId] = useState<string | null>(null);
  const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [message, setMessage] = useState('');

  const sessionRef = useRef<VotingSession | null>(session);
  useEffect(() => {
    sessionRef.current = session;
  }, [session]);

  useEffect(() => {
    if (sessionId) {
      fetchSession();
    }
  }, [sessionId]);

  // Connect to SignalR Hub
  useEffect(() => {
    if (!sessionId || !token) return;

    const newConnection = new HubConnectionBuilder()
      .withUrl(`${VOTING_HUB_URL}?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    const joinGroups = (conn: HubConnection, currentSession: VotingSession | null) => {
      if (!currentSession) return;
      if (currentSession.meetingId) {
        conn.invoke('JoinMeetingGroup', currentSession.meetingId).catch(err => console.error('JoinMeetingGroup error', err));
      }
      if (currentSession.id) {
        conn.invoke('JoinSessionGroup', currentSession.id).catch(err => console.error('JoinSessionGroup error', err));
      }
    };

    newConnection.onreconnected(() => {
      setIsConnected(true);
      if (sessionRef.current) {
        joinGroups(newConnection, sessionRef.current);
      }
    });

    newConnection.onreconnecting(() => {
      setIsConnected(false);
    });

    newConnection.start()
      .then(() => {
        setIsConnected(true);
        setConnection(newConnection);
        if (sessionRef.current) {
          joinGroups(newConnection, sessionRef.current);
        }
      })
      .catch(err => console.error('SignalR connection error', err));

    newConnection.on('OnSessionOpened', () => {
      setMessage('La votación ha sido abierta por el administrador.');
      fetchSession();
    });

    newConnection.on('OnVoteCast', data => {
      if (data?.liveStats) {
        setSession(prev => prev ? { ...prev, liveStats: data.liveStats } : null);
      }
    });

    newConnection.on('OnSessionClosed', data => {
      setMessage('La votación ha concluido.');
      fetchSession();
      if (data?.resultId) {
        fetchResult(data.resultId);
      }
    });

    return () => {
      newConnection.stop();
      setConnection(null);
      setIsConnected(false);
    };
  }, [sessionId, token]);

  // Ensure client joins SignalR groups as soon as session data is available
  useEffect(() => {
    if (connection && isConnected && session) {
      if (session.meetingId) {
        connection.invoke('JoinMeetingGroup', session.meetingId).catch(err => console.error('JoinMeetingGroup error', err));
      }
      if (session.id) {
        connection.invoke('JoinSessionGroup', session.id).catch(err => console.error('JoinSessionGroup error', err));
      }
    }
  }, [connection, isConnected, session?.meetingId, session?.id]);

  const fetchSession = async () => {
    try {
      const res = await votingApiClient.get<VotingSession>(`/sessions/${sessionId}`);
      setSession(res.data);

      if (user && res.data.ballots) {
        const myBallot = res.data.ballots.find(b => b.userId === user.id);
        if (myBallot && myBallot.status === 1 && myBallot.vote) {
          setConfirmedOptionId(myBallot.vote.selectedOptionId);
          setSelectedOptionId(myBallot.vote.selectedOptionId);
        }
      }

      if (res.data.state === VotingState.Closed) {
        fetchResultByProposal(res.data.proposalId);
      }
    } catch (err) {
      console.error('Error fetching voting session', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchResultByProposal = async (proposalId: string) => {
    try {
      const res = await votingApiClient.get<VotingResult>(`/results/${proposalId}`);
      setResult(res.data);
    } catch (err) {
      console.log('No saved result found yet');
    }
  };

  const fetchResult = async (resultId: string) => {
    try {
      const res = await votingApiClient.get<VotingResult>(`/results/${resultId}`);
      setResult(res.data);
    } catch (err) {
      console.error('Error fetching result', err);
    }
  };

  const handleOpenSession = async () => {
    if (!sessionId) return;
    try {
      await votingApiClient.post(`/sessions/${sessionId}/open`);
      fetchSession();
    } catch (err: any) {
      alert(err.response?.data || 'Error abriendo votación');
    }
  };

  const handleCloseSession = async () => {
    if (!sessionId) return;
    if (!window.confirm('¿Deseas cerrar definitivamente esta votación y calcular el resultado oficial?')) return;
    try {
      await votingApiClient.post(`/sessions/${sessionId}/close`);
      fetchSession();
    } catch (err: any) {
      alert(err.response?.data || 'Error cerrando votación');
    }
  };

  // Double Action Confirmation Barrier
  const handleSelectOption = (optionId: string) => {
    if (confirmedOptionId) return; // Already voted
    setSelectedOptionId(optionId);
  };

  const handleConfirmVote = async () => {
    if (!sessionId || !selectedOptionId) return;
    setVotingLoading(true);
    try {
      await votingApiClient.post(`/sessions/${sessionId}/vote`, { optionId: selectedOptionId });
      setConfirmedOptionId(selectedOptionId);
      setIsConfirmModalOpen(false);
      setMessage('¡Voto emitido y verificado en la cadena comunitaria!');
      fetchSession();
    } catch (err: any) {
      alert(err.response?.data || 'Error al emitir el voto');
    } finally {
      setVotingLoading(false);
    }
  };

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-secondary)' }}>Verificando credenciales de votación...</div>;
  }

  if (!session) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--danger)' }}>Sesión de votación no encontrada.</div>;
  }

  const isOpen = session.state === VotingState.Open;
  const isClosed = session.state === VotingState.Closed;

  const majorityType = session.majorityType ?? 1;
  const majorityPct = session.majorityPercentage;
  const getMajorityInfo = () => {
    switch (majorityType) {
      case 1:
        return { title: 'Mayoría Simple', desc: 'Requiere más votos A Favor que En Contra' };
      case 2:
        return { title: 'Mayoría Absoluta', desc: 'Requiere más del 50% de los votos emitidos' };
      case 3:
        return { title: `Mayoría Cualificada (${majorityPct || 66.67}%)`, desc: `Requiere al menos el ${majorityPct || 66.67}% de votos a favor` };
      default:
        return { title: 'Mayoría Simple', desc: 'Requiere más votos A Favor que En Contra' };
    }
  };
  const majorityInfo = getMajorityInfo();

  return (
    <div>
      {/* Breadcrumb */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
        <Link to="/communities" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>Comunidades</Link>
        <ChevronRight size={14} />
        <Link to={`/meetings/${session.meetingId}`} style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>Volver a la Reunión</Link>
        <ChevronRight size={14} />
        <span style={{ color: 'white', fontWeight: '600' }}>Votación Privada</span>
      </div>

      {/* 1. Privacy Header */}
      <PrivacyHeader
        communityName={session.meetingName}
        meetingTitle={session.title}
        isConnected={isConnected}
      />

      {/* Status Bar */}
      <div className="glass-panel" style={{ padding: '24px', marginBottom: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}>
          <div>
            <span className={`badge ${isOpen ? 'badge-success' : isClosed ? 'badge-warning' : 'badge-privacy'}`} style={{ marginBottom: '6px' }}>
              {isOpen ? '🟢 Votación Activa En Tiempo Real' : isClosed ? '🔴 Votación Finalizada' : '🟡 En Espera de Apertura'}
            </span>
            <h1 style={{ fontSize: '1.6rem', marginTop: '4px' }}>{session.title}</h1>
            {session.description && <p style={{ color: 'var(--text-secondary)', fontSize: '0.92rem', marginTop: '4px' }}>{session.description}</p>}
          </div>

          {/* Admin Controls */}
          {isAdmin && (
            <div style={{ display: 'flex', gap: '10px' }}>
              {!isOpen && !isClosed && (
                <button onClick={handleOpenSession} className="btn btn-success">
                  <Play size={18} /> Iniciar Votación
                </button>
              )}

              {isOpen && (
                <button onClick={handleCloseSession} className="btn btn-danger">
                  <Lock size={18} /> Cerrar y Calcular Resultado
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {message && (
        <div style={{ padding: '14px 18px', borderRadius: '12px', background: 'hsla(142, 71%, 45%, 0.15)', color: 'hsl(142, 80%, 65%)', border: '1px solid hsla(142, 71%, 45%, 0.35)', marginBottom: '24px', display: 'flex', alignItems: 'center', gap: '10px' }}>
          <CheckCircle2 size={18} /> {message}
        </div>
      )}

      {/* 2. Regla de Mayoría & Participación en Tiempo Real */}
      <div className="glass-panel" style={{ padding: '24px', marginBottom: '28px' }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '20px', alignItems: 'center' }}>
          <div>
            <span style={{ fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em', color: 'var(--accent)', fontWeight: '700' }}>
              Regla de Mayoría Requerida
            </span>
            <h3 style={{ fontSize: '1.15rem', color: 'white', marginTop: '2px' }}>
              {majorityInfo.title}
            </h3>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
              {majorityInfo.desc}
            </p>
          </div>

          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
              <span style={{ fontSize: '0.85rem', fontWeight: '600', color: 'var(--text-secondary)' }}>
                Participación en la Votación
              </span>
              <span style={{ fontSize: '0.9rem', fontWeight: '800', color: 'var(--accent)' }}>
                {session.liveStats?.totalVotesCast || 0} de {session.liveStats?.totalBallots || 0} Votos ({session.liveStats?.participationPercentage?.toFixed(1) || 0}%)
              </span>
            </div>
            <div className="progress-bar-bg">
              <div className="progress-bar-fill" style={{ width: `${session.liveStats?.participationPercentage || 0}%` }}></div>
            </div>
          </div>
        </div>
      </div>

      {/* Main Grid: Voting Cards vs Live Analytics */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(340px, 1fr))', gap: '24px' }}>
        
        {/* Voting Options (Mobile-First Interactive Cards) */}
        <div className="glass-panel" style={{ padding: '28px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
            <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Vote color="var(--accent)" /> Opciones de Voto
            </h3>
            {confirmedOptionId && (
              <span className="badge badge-success">
                <CheckCircle2 size={14} /> Voto Confirmado
              </span>
            )}
          </div>

          {!isOpen && !isClosed && (
            <div style={{ padding: '20px', borderRadius: '12px', background: 'hsla(38, 92%, 50%, 0.1)', color: 'hsl(38, 92%, 70%)', border: '1px solid hsla(38, 92%, 50%, 0.25)', fontSize: '0.9rem' }}>
              La votación aún no se ha abierto. Permanece en esta pantalla para participar cuando el administrador active la sesión.
            </div>
          )}

          {isOpen && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
              {session.options.map(opt => (
                <VotingOption
                  key={opt.id}
                  id={opt.id}
                  text={opt.label}
                  isSelected={selectedOptionId === opt.id}
                  onSelect={handleSelectOption}
                  disabled={!!confirmedOptionId}
                />
              ))}

              {/* 4. Double Action Confirmation Barrier */}
              {selectedOptionId && !confirmedOptionId && (
                <div style={{ marginTop: '16px', padding: '20px', borderRadius: '14px', background: 'hsla(250, 84%, 54%, 0.12)', border: '1px solid var(--accent)', animation: 'scaleIn 0.2s ease-out' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '12px' }}>
                    <ShieldCheck size={20} color="var(--accent)" />
                    <span style={{ fontSize: '0.9rem', fontWeight: '700' }}>Confirmación de Doble Acción</span>
                  </div>
                  <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
                    Has seleccionado: <strong style={{ color: 'white' }}>{session.options.find(o => o.id === selectedOptionId)?.label}</strong>. Confirma tu elección para registrar tu voto de forma definitiva.
                  </p>
                  <button onClick={() => setIsConfirmModalOpen(true)} className="btn btn-primary" style={{ width: '100%' }}>
                    <CheckCircle2 size={18} /> Confirmar Voto Definitivo
                  </button>
                </div>
              )}
            </div>
          )}

          {isClosed && (
            <div style={{ padding: '16px', borderRadius: '12px', background: 'rgba(255, 255, 255, 0.04)', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              Esta votación ha finalizado. Consulta los resultados oficiales a la derecha.
            </div>
          )}
        </div>

        {/* Real-time Analytics & Results Panel (SignalR + CSS-Pure Progress Bars) */}
        <div className="glass-panel" style={{ padding: '28px' }}>
          <h3 style={{ fontSize: '1.2rem', marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Award color="var(--accent)" /> {isClosed ? 'Resultado Oficial Auditado' : 'Escrutinio en Tiempo Real'}
          </h3>

          {result ? (
            <div>
              <div
                style={{
                  padding: '24px',
                  borderRadius: '16px',
                  background: result.approved
                    ? 'linear-gradient(135deg, hsla(142, 71%, 45%, 0.25) 0%, hsla(142, 90%, 55%, 0.15) 100%)'
                    : 'linear-gradient(135deg, hsla(0, 84%, 50%, 0.25) 0%, hsla(38, 90%, 50%, 0.15) 100%)',
                  border: result.approved ? '1px solid hsl(142, 71%, 45%)' : '1px solid hsl(0, 84%, 50%)',
                  marginBottom: '24px',
                  textAlign: 'center',
                  boxShadow: result.approved ? '0 0 30px hsla(142, 71%, 45%, 0.3)' : '0 0 30px hsla(0, 84%, 50%, 0.3)',
                }}
              >
                <span
                  style={{
                    fontSize: '0.85rem',
                    padding: '4px 12px',
                    borderRadius: '20px',
                    background: result.approved ? 'hsl(142, 71%, 45%)' : 'hsl(0, 84%, 50%)',
                    color: 'white',
                    fontWeight: '800',
                    textTransform: 'uppercase',
                    letterSpacing: '0.05em',
                  }}
                >
                  {result.approved ? 'Propuesta Aprobada ✓' : 'Propuesta Rechazada ❌'}
                </span>
                
                <h2 style={{ fontSize: '1.8rem', color: 'white', margin: '14px 0 6px' }}>{result.winningOptionLabel}</h2>
                
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', display: 'flex', justifyContent: 'center', gap: '16px', flexWrap: 'wrap', marginTop: '10px' }}>
                  <span>Cerrado por: {result.closedByUserName}</span>
                  <span>Criterio: {majorityInfo.title}</span>
                  <span>Total Votos Emitidos: {result.totalVotesCast}</span>
                </div>
              </div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                {session.options.map(opt => {
                  const votes = result.votesByOption[opt.id] || 0;
                  const percentage = result.totalVotesCast > 0 ? (votes / result.totalVotesCast) * 100 : 0;
                  return (
                    <div key={opt.id}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.88rem', marginBottom: '6px' }}>
                        <span style={{ fontWeight: '600' }}>{opt.label}</span>
                        <span style={{ fontWeight: '700', color: 'var(--accent)' }}>{votes} Votos ({percentage.toFixed(1)}%)</span>
                      </div>
                      <div className="progress-bar-bg">
                        <div className="progress-bar-fill" style={{ width: `${percentage}%` }}></div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '18px' }}>
              {session.options.map(opt => {
                const votes = session.liveStats?.votesByOption?.[opt.id] || 0;
                const total = session.liveStats?.totalVotesCast || 0;
                const percentage = total > 0 ? (votes / total) * 100 : 0;
                return (
                  <div key={opt.id}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.88rem', marginBottom: '6px' }}>
                      <span style={{ fontWeight: '600' }}>{opt.label}</span>
                      <span style={{ fontWeight: '700', color: 'var(--accent)' }}>{votes} Votos ({percentage.toFixed(1)}%)</span>
                    </div>
                    <div className="progress-bar-bg">
                      <div className="progress-bar-fill" style={{ width: `${percentage}%` }}></div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {/* Double Action Modal Confirmation */}
      {isConfirmModalOpen && (
        <div className="modal-overlay">
          <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '440px' }}>
            <div style={{ textAlign: 'center', marginBottom: '20px' }}>
              <div style={{ width: '48px', height: '48px', borderRadius: '50%', background: 'hsla(250, 84%, 54%, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 12px', border: '1px solid var(--accent)' }}>
                <AlertTriangle size={24} color="var(--accent)" />
              </div>
              <h3 style={{ fontSize: '1.25rem' }}>¿Confirmar Voto?</h3>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem', marginTop: '8px' }}>
                Estás a punto de emitir tu voto por: <strong style={{ color: 'white' }}>{session.options.find(o => o.id === selectedOptionId)?.label}</strong>.
              </p>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '4px' }}>
                Esta acción no se puede deshacer una vez registrada.
              </p>
            </div>

            <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
              <button type="button" onClick={() => setIsConfirmModalOpen(false)} className="btn btn-secondary" style={{ flex: 1 }}>
                Modificar
              </button>
              <button type="button" onClick={handleConfirmVote} disabled={votingLoading} className="btn btn-primary" style={{ flex: 1 }}>
                {votingLoading ? 'Registrando...' : 'Sí, Emitir Voto'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
