import React, { useState } from 'react';
import { documentApiClient } from '../api/client';
import { Document } from '../types';
import { X, Upload, FileText } from 'lucide-react';

interface UploadDocumentModalProps {
  proposalId: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const UploadDocumentModal: React.FC<UploadDocumentModalProps> = ({
  proposalId,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0];
      setFile(selectedFile);
      if (!title) {
        setTitle(selectedFile.name.split('.').slice(0, -1).join('.'));
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) {
      setError('Debes seleccionar un archivo para adjuntar');
      return;
    }
    if (!title.trim()) {
      setError('El título del documento es obligatorio');
      return;
    }

    setLoading(true);
    setError('');

    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', title.trim());
    if (description.trim()) {
      formData.append('description', description.trim());
    }

    try {
      await documentApiClient.post<Document>(`/documents/proposal/${proposalId}`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      setTitle('');
      setDescription('');
      setFile(null);
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.response?.data || 'Error al subir el documento');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Upload color="var(--accent)" /> Adjuntar Documento a Propuesta
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
            <label className="form-label">Seleccionar Archivo (PDF, DOCX, PNG, etc.)</label>
            <input
              type="file"
              className="form-input"
              onChange={handleFileChange}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Título del Documento</label>
            <input
              type="text"
              className="form-input"
              placeholder="Ej: Memoria Técnica del Proyecto"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Descripción</label>
            <textarea
              className="form-textarea"
              rows={2}
              placeholder="Breve resumen del contenido del archivo..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary" style={{ flex: 1 }}>
              Cancelar
            </button>
            <button type="submit" disabled={loading} className="btn btn-primary" style={{ flex: 1 }}>
              {loading ? 'Subiendo...' : 'Subir Archivo'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
