import React, { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import { CommunityInvitation } from '../types';
import { Link2, Copy, Check, X, Plus, Calendar, Users, ShieldCheck } from 'lucide-react';

interface InvitationManagerModalProps {
  communityId: string;
  communityName: string;
  isOpen: boolean;
  onClose: () => void;
}

export const InvitationManagerModal: React.FC<InvitationManagerModalProps> = ({
  communityId,
  communityName,
  isOpen,
  onClose,
}) => {
  const [invitations, setInvitations] = useState<CommunityInvitation[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [expiresInDays, setExpiresInDays] = useState<number>(7);
  const [maxUses, setMaxUses] = useState<number>(0);
  const [copiedToken, setCopiedToken] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && communityId) {
      fetchInvitations();
    }
  }, [isOpen, communityId]);

  const fetchInvitations = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<CommunityInvitation[]>(`/invitations/community/${communityId}`);
      setInvitations(res.data);
    } catch (err) {
      console.error('Error fetching invitations', err);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateLink = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreating(true);
    try {
      const res = await apiClient.post<CommunityInvitation>(`/invitations/community/${communityId}`, {
        communityId,
        expiresInDays: Number(expiresInDays),
        maxUses: Number(maxUses),
      });

      setInvitations(prev => [res.data, ...prev]);
      handleCopyLink(res.data.inviteUrl, res.data.token);
    } catch (err) {
      alert('Error generando enlace de invitación');
    } finally {
      setCreating(false);
    }
  };

  const handleCopyLink = (url: string, token: string) => {
    navigator.clipboard.writeText(url);
    setCopiedToken(token);
    setTimeout(() => setCopiedToken(null), 3000);
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '640px', width: '90%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Link2 color="var(--accent)" /> Enlaces de Invitación - {communityName}
          </h3>
          <button onClick={onClose} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px' }}>
            <X size={16} />
          </button>
        </div>

        {/* Generate New Link Form */}
        <form onSubmit={handleGenerateLink} style={{ padding: '16px', borderRadius: '14px', background: 'rgba(255, 255, 255, 0.03)', border: '1px solid var(--border-glass)', marginBottom: '24px' }}>
          <h4 style={{ fontSize: '0.98rem', color: 'var(--accent)', marginBottom: '12px' }}>Generar Nuevo Enlace de Registro</h4>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '14px', marginBottom: '16px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                Caducidad del Enlace
              </label>
              <select
                className="form-input"
                value={expiresInDays}
                onChange={e => setExpiresInDays(Number(e.target.value))}
              >
                <option value={7}>7 días (Estándar)</option>
                <option value={14}>14 días</option>
                <option value={30}>30 días</option>
                <option value={0}>Sin expiración</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                Límite de Usos
              </label>
              <select
                className="form-input"
                value={maxUses}
                onChange={e => setMaxUses(Number(e.target.value))}
              >
                <option value={0}>Usos ilimitados (Para grupo)</option>
                <option value={1}>1 solo uso (Invitación personal)</option>
                <option value={5}>5 usos</option>
                <option value={10}>10 usos</option>
              </select>
            </div>
          </div>

          <button type="submit" disabled={creating} className="btn btn-primary btn-sm" style={{ width: '100%', justifyContent: 'center' }}>
            <Plus size={16} /> {creating ? 'Generando Enlace...' : 'Generar y Copiar Nuevo Enlace'}
          </button>
        </form>

        {/* Existing Invitations List */}
        <h4 style={{ fontSize: '1rem', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          Enlaces Generados ({invitations.length})
        </h4>

        {loading ? (
          <div style={{ textAlign: 'center', padding: '20px', color: 'var(--text-secondary)' }}>Cargando enlaces...</div>
        ) : invitations.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '20px', color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
            Aún no se ha generado ningún enlace de invitación para esta comunidad.
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxHeight: '280px', overflowY: 'auto', paddingRight: '4px' }}>
            {invitations.map(inv => (
              <div
                key={inv.id}
                style={{
                  padding: '12px 16px',
                  borderRadius: '10px',
                  background: 'var(--bg-surface)',
                  border: '1px solid var(--border-glass)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  flexWrap: 'wrap',
                  gap: '12px',
                }}
              >
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span className={`badge ${inv.isValid ? 'badge-success' : 'badge-warning'}`} style={{ fontSize: '0.72rem' }}>
                      {inv.isValid ? 'Enlace Activo' : 'Expirado / Agotado'}
                    </span>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                      Usos: {inv.usesCount}{inv.maxUses > 0 ? `/${inv.maxUses}` : ' (Ilimitado)'}
                    </span>
                  </div>

                  <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
                    Creado por {inv.createdByUserName} • {inv.expiresAt ? `Expira: ${new Date(inv.expiresAt).toLocaleDateString()}` : 'Sin caducidad'}
                  </div>
                </div>

                <button
                  type="button"
                  onClick={() => handleCopyLink(inv.inviteUrl, inv.token)}
                  className={`btn ${copiedToken === inv.token ? 'btn-success' : 'btn-secondary'} btn-sm`}
                  style={{ fontSize: '0.78rem' }}
                >
                  {copiedToken === inv.token ? (
                    <><Check size={14} /> ¡Copiado!</>
                  ) : (
                    <><Copy size={14} /> Copiar Enlace</>
                  )}
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
