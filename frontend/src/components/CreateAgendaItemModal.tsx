import React, { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import { X, ListOrdered } from 'lucide-react';

interface CreateAgendaItemModalProps {
  meetingId: string;
  nextOrder?: number;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateAgendaItemModal: React.FC<CreateAgendaItemModalProps> = ({
  meetingId,
  nextOrder = 1,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [order, setOrder] = useState(nextOrder);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (isOpen) {
      setOrder(nextOrder);
    }
  }, [isOpen, nextOrder]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('El título del punto del día es obligatorio');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await apiClient.post('/agenda-items', {
        meetingId,
        title: title.trim(),
        description: description.trim() || null,
        order,
      });

      setTitle('');
      setDescription('');
      setOrder(1);
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data || 'Error al crear el punto del día');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <ListOrdered color="var(--accent)" /> Nuevo Punto del Orden del Día (AgendaItem)
          </h3>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer' }}>
            <X size={20} />
          </button>
        </div>

        {error && (
          <div style={{ padding: '12px', borderRadius: '8px', background: 'hsla(350, 89%, 60%, 0.15)', color: 'hsl(350, 89%, 70%)', marginBottom: '16px', fontSize: '0.85rem' }}>
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Título del Punto</label>
            <input
              type="text"
              className="form-input"
              placeholder="Ej: Punto 1: Aprobación de Cuentas Anuales"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Descripción o Notas</label>
            <textarea
              className="form-textarea"
              rows={3}
              placeholder="Detalles relativos a este punto del orden del día..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Orden en el Día</label>
            <input
              type="number"
              className="form-input"
              min={1}
              value={order}
              onChange={(e) => setOrder(parseInt(e.target.value) || 1)}
            />
          </div>

          <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary" style={{ flex: 1 }}>
              Cancelar
            </button>
            <button type="submit" disabled={loading} className="btn btn-primary" style={{ flex: 1 }}>
              {loading ? 'Creando...' : 'Guardar Punto'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
