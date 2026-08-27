import React, { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import { Proposal } from '../types';
import { X, Edit3, Plus, Trash2 } from 'lucide-react';

interface EditProposalModalProps {
  proposal: Proposal;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditProposalModal: React.FC<EditProposalModalProps> = ({
  proposal,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [title, setTitle] = useState(proposal.title);
  const [description, setDescription] = useState(proposal.description || '');
  const [options, setOptions] = useState<string[]>(
    proposal.options && proposal.options.length > 0
      ? proposal.options.map((o) => o.label)
      : ['A favor', 'En contra', 'Abstención']
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    setTitle(proposal.title);
    setDescription(proposal.description || '');
    setOptions(
      proposal.options && proposal.options.length > 0
        ? proposal.options.map((o) => o.label)
        : ['A favor', 'En contra', 'Abstención']
    );
  }, [proposal]);

  if (!isOpen) return null;

  const handleAddOption = () => {
    setOptions([...options, '']);
  };

  const handleRemoveOption = (index: number) => {
    if (options.length <= 2) {
      alert('Debe haber al menos 2 opciones de voto');
      return;
    }
    setOptions(options.filter((_, i) => i !== index));
  };

  const handleOptionChange = (index: number, value: string) => {
    const newOptions = [...options];
    newOptions[index] = value;
    setOptions(newOptions);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('El título de la propuesta es obligatorio');
      return;
    }

    const validOptions = options.map((o) => o.trim()).filter((o) => o.length > 0);
    if (validOptions.length < 2) {
      setError('Debes especificar al menos 2 opciones de voto válidas');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await apiClient.put(`/proposals/${proposal.id}`, {
        title: title.trim(),
        description: description.trim() || null,
        order: proposal.order || 1,
        majorityType: proposal.majorityType,
        majorityPercentage: proposal.majorityPercentage,
        options: validOptions,
      });

      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data || 'Error al actualizar la propuesta');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Edit3 color="var(--accent)" /> Editar Propuesta de Votación
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
            <label className="form-label">Título de la Propuesta</label>
            <input
              type="text"
              className="form-input"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Descripción</label>
            <textarea
              className="form-textarea"
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div style={{ marginTop: '20px' }}>
            <label className="form-label" style={{ display: 'block', marginBottom: '10px' }}>Opciones de Votación</label>
            {options.map((opt, idx) => (
              <div key={idx} style={{ display: 'flex', gap: '8px', marginBottom: '8px' }}>
                <input
                  type="text"
                  className="form-input"
                  placeholder={`Opción ${idx + 1}`}
                  value={opt}
                  onChange={(e) => handleOptionChange(idx, e.target.value)}
                  required
                />
                {options.length > 2 && (
                  <button
                    type="button"
                    onClick={() => handleRemoveOption(idx)}
                    className="btn btn-secondary btn-sm"
                    style={{ color: 'var(--danger)' }}
                  >
                    <Trash2 size={16} />
                  </button>
                )}
              </div>
            ))}

            <button type="button" onClick={handleAddOption} className="btn btn-secondary btn-sm" style={{ marginTop: '6px' }}>
              <Plus size={14} /> Añadir Otra Opción
            </button>
          </div>

          <div style={{ display: 'flex', gap: '12px', marginTop: '28px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary" style={{ flex: 1 }}>
              Cancelar
            </button>
            <button type="submit" disabled={loading} className="btn btn-primary" style={{ flex: 1 }}>
              {loading ? 'Guardando...' : 'Guardar Cambios'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
