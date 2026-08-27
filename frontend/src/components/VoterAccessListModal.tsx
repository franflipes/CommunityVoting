import React, { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import { MeetingVoterAccessDto } from '../types';
import { KeyRound, Copy, RefreshCw, X, ShieldAlert, Check, ExternalLink, UserCheck } from 'lucide-react';

interface VoterAccessListModalProps {
  meetingId: string;
  meetingTitle: string;
  isOpen: boolean;
  onClose: () => void;
}

export const VoterAccessListModal: React.FC<VoterAccessListModalProps> = ({
  meetingId,
  meetingTitle,
  isOpen,
  onClose
}) => {
  const [accesses, setAccesses] = useState<MeetingVoterAccessDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [copiedAll, setCopiedAll] = useState(false);

  useEffect(() => {
    if (isOpen && meetingId) {
      fetchAccesses();
    }
  }, [isOpen, meetingId]);

  const fetchAccesses = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<MeetingVoterAccessDto[]>(`/meetings/${meetingId}/voter-accesses`);
      setAccesses(res.data);
    } catch (err) {
      console.error('Error fetching voter accesses', err);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateAll = async () => {
    setLoading(true);
    try {
      const res = await apiClient.post<MeetingVoterAccessDto[]>(`/meetings/${meetingId}/voter-accesses/generate-all`);
      setAccesses(res.data);
    } catch (err) {
      console.error('Error generating voter accesses', err);
      alert('Error generando los accesos de la reunión.');
    } finally {
      setLoading(false);
    }
  };

  const handleRevoke = async (userId: string) => {
    if (!window.confirm('¿Estás seguro de revocar el acceso de este votante? El enlace y código dejarán de funcionar.')) return;
    try {
      await apiClient.post(`/meetings/${meetingId}/voter-accesses/revoke/${userId}`);
      fetchAccesses();
    } catch (err) {
      alert('Error revocando el acceso.');
    }
  };

  const handleRegenerate = async (userId: string) => {
    try {
      const res = await apiClient.post<MeetingVoterAccessDto>(`/meetings/${meetingId}/voter-accesses/regenerate/${userId}`);
      setAccesses(prev => prev.map(a => a.userId === userId ? res.data : a));
    } catch (err) {
      alert('Error regenerando el acceso.');
    }
  };

  const handleCopySingle = (access: MeetingVoterAccessDto) => {
    const codeText = access.code && access.code !== '[PROTECTED_CODE]' ? access.code : 'Código registrado';
    const textToCopy = `Acceso a Reunión "${meetingTitle}":\nEnlace: ${access.accessUrl}\nCódigo de Acceso: ${codeText}`;
    navigator.clipboard.writeText(textToCopy);
    setCopiedId(access.id);
    setTimeout(() => setCopiedId(null), 2500);
  };

  const handleCopyAll = () => {
    const formattedList = accesses.map(a => {
      const codeText = a.code && a.code !== '[PROTECTED_CODE]' ? a.code : '(Código existente)';
      return `👤 ${a.userName} (${a.userEmail})\n   Enlace: ${a.accessUrl}\n   Código: ${codeText}\n`;
    }).join('\n');

    const textToCopy = `📋 ACCESOS DE VOTANTES - ${meetingTitle.toUpperCase()}\n\n${formattedList}`;
    navigator.clipboard.writeText(textToCopy);
    setCopiedAll(true);
    setTimeout(() => setCopiedAll(false), 2500);
  };

  if (!isOpen) return null;

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      backgroundColor: 'rgba(0, 0, 0, 0.75)',
      backdropFilter: 'blur(8px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      zIndex: 1100,
      padding: '20px'
    }}>
      <div className="glass-panel" style={{
        maxWidth: '750px',
        width: '100%',
        maxHeight: '90vh',
        display: 'flex',
        flexDirection: 'column',
        padding: '28px',
        position: 'relative'
      }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '20px' }}>
          <div>
            <span className="badge badge-privacy" style={{ marginBottom: '6px', display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
              <KeyRound size={12} /> Credenciales de Votantes
            </span>
            <h2 style={{ fontSize: '1.4rem', fontWeight: '800' }}>Accesos por Enlace + Código</h2>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '4px' }}>
              Reunión: <strong style={{ color: 'var(--text-primary)' }}>{meetingTitle}</strong>
            </p>
          </div>

          <button
            onClick={onClose}
            style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer', padding: '4px' }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Action bar */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '12px', marginBottom: '16px', flexWrap: 'wrap' }}>
          <button
            onClick={handleCopyAll}
            disabled={accesses.length === 0}
            className="btn btn-secondary btn-sm"
            style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}
          >
            {copiedAll ? <Check size={14} color="#34d399" /> : <Copy size={14} />}
            {copiedAll ? '¡Todos Copiados!' : 'Copiar Todos los Accesos (WhatsApp / Email)'}
          </button>

          <button
            onClick={handleGenerateAll}
            className="btn btn-secondary btn-sm"
            style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}
          >
            <RefreshCw size={14} /> Sincronizar / Generar Faltantes
          </button>
        </div>

        {/* List content */}
        <div style={{ flex: 1, overflowY: 'auto', paddingRight: '4px', display: 'flex', flexDirection: 'column', gap: '12px' }}>
          {loading ? (
            <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-muted)' }}>Cargando accesos de la reunión...</div>
          ) : accesses.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-muted)' }}>
              No se encontraron accesos generados para esta reunión. Haz clic en "Sincronizar" para crearlos.
            </div>
          ) : (
            accesses.map(access => (
              <div
                key={access.id}
                style={{
                  padding: '16px',
                  borderRadius: '12px',
                  background: access.isRevoked ? 'rgba(239, 68, 68, 0.06)' : 'var(--bg-surface, rgba(255, 255, 255, 0.04))',
                  border: `1px solid ${access.isRevoked ? 'rgba(239, 68, 68, 0.3)' : 'var(--border-glass, rgba(255, 255, 255, 0.1))'}`,
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  flexWrap: 'wrap',
                  gap: '12px'
                }}
              >
                <div style={{ flex: 1, minWidth: '240px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <strong style={{ fontSize: '0.95rem' }}>{access.userName}</strong>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>({access.userEmail})</span>
                    {access.isRevoked ? (
                      <span className="badge badge-warning" style={{ fontSize: '0.72rem' }}>Revocado</span>
                    ) : (
                      <span className="badge badge-success" style={{ fontSize: '0.72rem' }}>Activo</span>
                    )}
                  </div>

                  <div style={{ marginTop: '8px', fontSize: '0.82rem', display: 'flex', flexDirection: 'column', gap: '4px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                      <span style={{ color: 'var(--text-muted)' }}>Enlace:</span>
                      <a href={access.accessUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--primary)', textDecoration: 'underline', wordBreak: 'break-all' }}>
                        {access.accessUrl}
                      </a>
                      <ExternalLink size={12} color="var(--primary)" />
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      <div>
                        <span style={{ color: 'var(--text-muted)' }}>Código: </span>
                        <strong style={{ fontFamily: 'monospace', letterSpacing: '0.1em', fontSize: '0.9rem', color: 'var(--secondary)' }}>
                          {access.code && access.code !== '[PROTECTED_CODE]' ? access.code : '•••••• (Registrado)'}
                        </strong>
                      </div>
                      {access.lastUsedAt && (
                        <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                          <UserCheck size={12} color="#34d399" /> Último uso: {new Date(access.lastUsedAt).toLocaleString()}
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                  <button
                    onClick={() => handleCopySingle(access)}
                    className="btn btn-secondary btn-sm"
                    title="Copiar enlace y código"
                  >
                    {copiedId === access.id ? <Check size={14} color="#34d399" /> : <Copy size={14} />}
                    {copiedId === access.id ? 'Copiado' : 'Copiar'}
                  </button>

                  <button
                    onClick={() => handleRegenerate(access.userId)}
                    className="btn btn-secondary btn-sm"
                    title="Regenerar nuevo enlace y código"
                  >
                    <RefreshCw size={14} /> Regenerar
                  </button>

                  {!access.isRevoked && (
                    <button
                      onClick={() => handleRevoke(access.userId)}
                      className="btn btn-secondary btn-sm"
                      style={{ color: 'var(--danger)' }}
                      title="Revocar credencial"
                    >
                      <ShieldAlert size={14} /> Revocar
                    </button>
                  )}
                </div>
              </div>
            ))
          )}
        </div>

        {/* Footer */}
        <div style={{ marginTop: '20px', paddingTop: '16px', borderTop: '1px solid var(--border-glass)', textAlign: 'right' }}>
          <button onClick={onClose} className="btn btn-secondary">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};
