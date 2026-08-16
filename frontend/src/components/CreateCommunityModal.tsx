import React, { useState } from 'react';
import { X, Building2 } from 'lucide-react';
import { apiClient } from '../api/client';
import { Community } from '../types';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (community: Community) => void;
}

export const CreateCommunityModal: React.FC<Props> = ({ isOpen, onClose, onSuccess }) => {
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [cif, setCif] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !address) {
      setError('El nombre y la dirección son obligatorios.');
      return;
    }
    setLoading(true);
    setError('');
    try {
      const res = await apiClient.post<Community>('/communities', { name, address, cif });
      onSuccess(res.data);
      setName('');
      setAddress('');
      setCif('');
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al crear la comunidad');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Building2 size={24} color="var(--primary)" />
            <h3 style={{ fontSize: '1.2rem' }}>Nueva Comunidad</h3>
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
            <X size={20} />
          </button>
        </div>

        {error && <div style={{ padding: '10px 14px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', border: '1px solid rgba(239, 68, 68, 0.3)', marginBottom: '16px', fontSize: '0.85rem' }}>{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Nombre de la Comunidad</label>
            <input className="form-input" type="text" placeholder="Ej. Residencial Las Flores" value={name} onChange={e => setName(e.target.value)} required />
          </div>

          <div className="form-group">
            <label className="form-label">Dirección</label>
            <input className="form-input" type="text" placeholder="Ej. Av. Principal 123" value={address} onChange={e => setAddress(e.target.value)} required />
          </div>

          <div className="form-group">
            <label className="form-label">CIF / NIF (Opcional)</label>
            <input className="form-input" type="text" placeholder="Ej. H12345678" value={cif} onChange={e => setCif(e.target.value)} />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '24px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary">Cancelar</button>
            <button type="submit" disabled={loading} className="btn btn-primary">
              {loading ? 'Creando...' : 'Crear Comunidad'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
