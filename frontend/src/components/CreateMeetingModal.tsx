import React, { useState } from 'react';
import { X, Calendar } from 'lucide-react';
import { apiClient } from '../api/client';
import { Meeting, MeetingType } from '../types';

interface Props {
  isOpen: boolean;
  communityId: string;
  onClose: () => void;
  onSuccess: (meeting: Meeting) => void;
}

export const CreateMeetingModal: React.FC<Props> = ({ isOpen, communityId, onClose, onSuccess }) => {
  const [title, setTitle] = useState('');
  const [type, setType] = useState<MeetingType>(MeetingType.Ordinary);
  const [location, setLocation] = useState('Sala de Comunidad / Online');
  const [scheduledAt, setScheduledAt] = useState('');
  const [secondCallAt, setSecondCallAt] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title || !scheduledAt) {
      setError('El título y la fecha de convocatoria son obligatorios.');
      return;
    }
    setLoading(true);
    setError('');
    try {
      const res = await apiClient.post<Meeting>('/meetings', {
        communityId,
        title,
        type: Number(type),
        location,
        scheduledAt: new Date(scheduledAt).toISOString(),
        secondCallAt: secondCallAt ? new Date(secondCallAt).toISOString() : null,
        isTransparent: true
      });
      onSuccess(res.data);
      setTitle('');
      setScheduledAt('');
      setSecondCallAt('');
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al crear la reunión');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Calendar size={24} color="var(--primary)" />
            <h3 style={{ fontSize: '1.2rem' }}>Nueva Convocar Junta / Reunión</h3>
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
            <X size={20} />
          </button>
        </div>

        {error && <div style={{ padding: '10px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', border: '1px solid rgba(239, 68, 68, 0.3)', marginBottom: '16px', fontSize: '0.85rem' }}>{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Título de la Reunión</label>
            <input className="form-input" type="text" placeholder="Ej. Junta General Ordinaria 2026" value={title} onChange={e => setTitle(e.target.value)} required />
          </div>

          <div className="form-group">
            <label className="form-label">Tipo de Convocatoria</label>
            <select className="form-select" value={type} onChange={e => setType(Number(e.target.value))}>
              <option value={MeetingType.Ordinary}>Ordinaria</option>
              <option value={MeetingType.Extraordinary}>Extraordinaria</option>
            </select>
          </div>

          <div className="form-group">
            <label className="form-label">Ubicación / Plataforma</label>
            <input className="form-input" type="text" value={location} onChange={e => setLocation(e.target.value)} required />
          </div>

          <div className="form-group">
            <label className="form-label">1ª Convocatoria (Fecha y Hora)</label>
            <input className="form-input" type="datetime-local" value={scheduledAt} onChange={e => setScheduledAt(e.target.value)} required />
          </div>

          <div className="form-group">
            <label className="form-label">2ª Convocatoria (Opcional)</label>
            <input className="form-input" type="datetime-local" value={secondCallAt} onChange={e => setSecondCallAt(e.target.value)} />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '24px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary">Cancelar</button>
            <button type="submit" disabled={loading} className="btn btn-primary">
              {loading ? 'Creando...' : 'Crear Reunión'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
