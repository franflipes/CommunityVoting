import React, { useState, useEffect } from 'react';
import { Meeting, MeetingType } from '../types';
import { apiClient } from '../api/client';
import { Calendar, MapPin, Edit3, X, Save } from 'lucide-react';

interface EditMeetingModalProps {
  meeting: Meeting;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (updatedMeeting: Meeting) => void;
}

export const EditMeetingModal: React.FC<EditMeetingModalProps> = ({
  meeting,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [title, setTitle] = useState(meeting.title);
  const [type, setType] = useState<MeetingType>(meeting.type);
  const [location, setLocation] = useState(meeting.location);
  const [scheduledAt, setScheduledAt] = useState('');
  const [secondCallAt, setSecondCallAt] = useState('');
  const [isTransparent, setIsTransparent] = useState(meeting.isTransparent);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (meeting) {
      setTitle(meeting.title);
      setType(meeting.type);
      setLocation(meeting.location);
      setIsTransparent(meeting.isTransparent);

      if (meeting.scheduledAt) {
        const d = new Date(meeting.scheduledAt);
        setScheduledAt(formatDateTimeLocal(d));
      }
      if (meeting.secondCallAt) {
        const d = new Date(meeting.secondCallAt);
        setSecondCallAt(formatDateTimeLocal(d));
      } else {
        setSecondCallAt('');
      }
    }
  }, [meeting]);

  const formatDateTimeLocal = (date: Date) => {
    const pad = (n: number) => (n < 10 ? '0' + n : n);
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  };

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        title,
        type: Number(type),
        location,
        scheduledAt: new Date(scheduledAt).toISOString(),
        secondCallAt: secondCallAt ? new Date(secondCallAt).toISOString() : null,
        isTransparent,
      };

      const res = await apiClient.put<Meeting>(`/meetings/${meeting.id}`, payload);
      onSuccess(res.data);
      onClose();
    } catch (err: any) {
      alert(err.response?.data || 'Error al actualizar la reunión');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '520px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px', margin: 0 }}>
            <Edit3 color="var(--accent)" /> Editar Datos y Fechas de la Junta
          </h3>
          <button onClick={onClose} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px' }}>
            <X size={16} />
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
              Título de la Junta *
            </label>
            <input
              type="text"
              className="form-input"
              value={title}
              onChange={e => setTitle(e.target.value)}
              required
            />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                Tipo de Convocatoria
              </label>
              <select
                className="form-input"
                value={type}
                onChange={e => setType(Number(e.target.value) as MeetingType)}
              >
                <option value={MeetingType.Ordinary}>Ordinaria</option>
                <option value={MeetingType.Extraordinary}>Extraordinaria</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                Ubicación / Sala *
              </label>
              <input
                type="text"
                className="form-input"
                value={location}
                onChange={e => setLocation(e.target.value)}
                required
              />
            </div>
          </div>

          <div>
            <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
              <Calendar size={14} style={{ verticalAlign: 'middle', marginRight: '4px' }} />
              Fecha y Hora (1ª Convocatoria) *
            </label>
            <input
              type="datetime-local"
              className="form-input"
              value={scheduledAt}
              onChange={e => setScheduledAt(e.target.value)}
              required
            />
          </div>

          <div>
            <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
              <Calendar size={14} style={{ verticalAlign: 'middle', marginRight: '4px' }} />
              Fecha y Hora (2ª Convocatoria - Opcional)
            </label>
            <input
              type="datetime-local"
              className="form-input"
              value={secondCallAt}
              onChange={e => setSecondCallAt(e.target.value)}
            />
          </div>

          <label style={{ display: 'flex', alignItems: 'center', gap: '10px', fontSize: '0.88rem', cursor: 'pointer' }}>
            <input
              type="checkbox"
              checked={isTransparent}
              onChange={e => setIsTransparent(e.target.checked)}
            />
            Muestra estado de asistencia y quórum en tiempo real a todos los propietarios
          </label>

          <div style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end', marginTop: '12px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary">
              Cancelar
            </button>
            <button type="submit" disabled={saving} className="btn btn-primary">
              <Save size={16} /> {saving ? 'Guardando...' : 'Guardar Cambios'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
