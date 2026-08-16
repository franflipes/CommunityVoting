import React, { useEffect, useState } from 'react';
import { MeetingParticipant } from '../types';
import { apiClient } from '../api/client';
import { Users, UserCheck, X, Clock, Mail } from 'lucide-react';

interface AttendanceListModalProps {
  meetingId: string;
  meetingTitle: string;
  isOpen: boolean;
  onClose: () => void;
}

export const AttendanceListModal: React.FC<AttendanceListModalProps> = ({
  meetingId,
  meetingTitle,
  isOpen,
  onClose,
}) => {
  const [participants, setParticipants] = useState<MeetingParticipant[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (isOpen && meetingId) {
      fetchParticipants();
    }
  }, [isOpen, meetingId]);

  const fetchParticipants = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get<MeetingParticipant[]>(`/meetings/${meetingId}/participants`);
      setParticipants(res.data);
    } catch (err) {
      console.error('Error fetching participants', err);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '640px', width: '90%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <div>
            <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px', margin: 0 }}>
              <UserCheck color="hsl(142, 71%, 65%)" /> Registro de Asistencia Confirmada
            </h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: '4px' }}>
              Listado de propietarios y representantes que han marcado asistencia para: <strong>{meetingTitle}</strong>
            </p>
          </div>
          <button onClick={onClose} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px' }}>
            <X size={16} />
          </button>
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-secondary)' }}>Cargando registro de asistencia...</div>
        ) : participants.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px', color: 'var(--text-secondary)' }}>
            <Users size={36} style={{ opacity: 0.4, marginBottom: '10px' }} />
            <p style={{ fontSize: '0.95rem' }}>Ningún asistente ha registrado aún su presencia en esta reunión.</p>
          </div>
        ) : (
          <div style={{ overflowX: 'auto', maxHeight: '360px' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border-glass)', textAlign: 'left', color: 'var(--text-secondary)' }}>
                  <th style={{ padding: '10px 12px' }}>Propietario / Asistente</th>
                  <th style={{ padding: '10px 12px' }}>Email</th>
                  <th style={{ padding: '10px 12px' }}>Fecha/Hora de Registro</th>
                  <th style={{ padding: '10px 12px', textAlign: 'center' }}>Estado</th>
                </tr>
              </thead>
              <tbody>
                {participants.map((p) => (
                  <tr key={p.id} style={{ borderBottom: '1px solid var(--border-glass)' }}>
                    <td style={{ padding: '12px', fontWeight: '600' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <div style={{ width: '28px', height: '28px', borderRadius: '50%', background: 'hsl(142, 71%, 45%)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                          <UserCheck size={14} color="white" />
                        </div>
                        {p.userName} {p.userLastName}
                      </div>
                    </td>
                    <td style={{ padding: '12px', color: 'var(--text-secondary)' }}>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Mail size={13} color="var(--accent)" /> {p.userEmail}
                      </span>
                    </td>
                    <td style={{ padding: '12px', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
                      <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                        <Clock size={13} color="var(--accent)" /> {new Date(p.joinedAt).toLocaleString()}
                      </span>
                    </td>
                    <td style={{ padding: '12px', textAlign: 'center' }}>
                      <span className={`badge ${p.isPresent ? 'badge-success' : 'badge-warning'}`} style={{ fontSize: '0.74rem' }}>
                        {p.isPresent ? 'Presente' : 'Ausente'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: '20px', paddingTop: '14px', borderTop: '1px solid var(--border-glass)' }}>
          <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            Total asistentes registrados: <strong style={{ color: 'var(--text-primary)' }}>{participants.length}</strong>
          </span>
          <button onClick={onClose} className="btn btn-secondary">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
};
