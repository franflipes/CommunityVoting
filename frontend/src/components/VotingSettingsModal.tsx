import React, { useState } from 'react';
import { VotingSettings, QuorumType, MajorityType, AbstentionPolicy } from '../types';
import { ShieldCheck, X } from 'lucide-react';

interface VotingSettingsModalProps {
  isOpen: boolean;
  onClose: () => void;
  settings?: VotingSettings;
  onSave: (updated: Partial<VotingSettings>) => Promise<void>;
  title?: string;
}

export const VotingSettingsModal: React.FC<VotingSettingsModalProps> = ({
  isOpen,
  onClose,
  settings,
  onSave,
  title = 'Configuración de Reglas de Votación',
}) => {
  const [quorumEnabled, setQuorumEnabled] = useState(settings?.quorumEnabled ?? true);
  const [quorumPercentage, setQuorumPercentage] = useState(settings?.quorumPercentage ?? 50);
  const [requireQuorumForVoting, setRequireQuorumForVoting] = useState(settings?.requireQuorumForVoting ?? true);
  const [defaultMajorityType, setDefaultMajorityType] = useState<MajorityType>(settings?.defaultMajorityType ?? MajorityType.SimpleMajority);
  const [defaultMajorityPercentage, setDefaultMajorityPercentage] = useState<number | undefined>(settings?.defaultMajorityPercentage ?? 66.67);
  const [abstentionPolicy, setAbstentionPolicy] = useState<AbstentionPolicy>(settings?.abstentionPolicy ?? AbstentionPolicy.Excluded);
  const [saving, setSaving] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await onSave({
        quorumEnabled,
        quorumType: QuorumType.PercentageOfEligibleMembers,
        quorumPercentage: Number(quorumPercentage),
        requireQuorumForVoting,
        defaultMajorityType: Number(defaultMajorityType),
        defaultMajorityPercentage: defaultMajorityType === MajorityType.QualifiedMajority ? Number(defaultMajorityPercentage) : undefined,
        abstentionPolicy: Number(abstentionPolicy),
      });
      onClose();
    } catch (err) {
      alert('Error guardando configuración de votación');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '520px', width: '90%' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <ShieldCheck color="var(--accent)" /> {title}
          </h3>
          <button onClick={onClose} className="btn btn-secondary btn-sm" style={{ padding: '4px 8px' }}>
            <X size={16} />
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          {/* Section 1: Quorum */}
          <div style={{ padding: '16px', borderRadius: '12px', background: 'rgba(255, 255, 255, 0.03)', border: '1px solid var(--border-glass)' }}>
            <h4 style={{ fontSize: '1rem', color: 'var(--accent)', marginBottom: '12px' }}>Reglas de Quórum Mínimo</h4>
            
            <label style={{ display: 'flex', alignItems: 'center', gap: '10px', fontSize: '0.9rem', marginBottom: '12px', cursor: 'pointer' }}>
              <input
                type="checkbox"
                checked={quorumEnabled}
                onChange={e => setQuorumEnabled(e.target.checked)}
              />
              Habilitar exigencia de Quórum
            </label>

            {quorumEnabled && (
              <>
                <div style={{ marginBottom: '12px' }}>
                  <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                    Porcentaje de Quórum Mínimo Requerido (%)
                  </label>
                  <input
                    type="number"
                    min="1"
                    max="100"
                    step="1"
                    className="form-input"
                    value={quorumPercentage}
                    onChange={e => setQuorumPercentage(Number(e.target.value))}
                    required
                  />
                </div>

                <label style={{ display: 'flex', alignItems: 'center', gap: '10px', fontSize: '0.88rem', cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={requireQuorumForVoting}
                    onChange={e => setRequireQuorumForVoting(e.target.checked)}
                  />
                  Impedir inicio de votación si no se alcanza el Quórum
                </label>
              </>
            )}
          </div>

          {/* Section 2: Majority */}
          <div style={{ padding: '16px', borderRadius: '12px', background: 'rgba(255, 255, 255, 0.03)', border: '1px solid var(--border-glass)' }}>
            <h4 style={{ fontSize: '1rem', color: 'var(--accent)', marginBottom: '12px' }}>Reglas de Mayoría para Aprobación</h4>

            <div style={{ marginBottom: '14px' }}>
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                Tipo de Mayoría por Defecto
              </label>
              <select
                className="form-input"
                value={defaultMajorityType}
                onChange={e => setDefaultMajorityType(Number(e.target.value) as MajorityType)}
              >
                <option value={MajorityType.SimpleMajority}>Mayoría Simple (Votos A favor &gt; En contra)</option>
                <option value={MajorityType.MajorityOfVotesCast}>Mayoría de Votos Emitidos (&gt; 50% votos)</option>
                <option value={MajorityType.QualifiedMajority}>Mayoría Cualificada (Porcentaje especificado)</option>
              </select>
            </div>

            {defaultMajorityType === MajorityType.QualifiedMajority && (
              <div style={{ marginBottom: '14px' }}>
                <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                  Porcentaje Requerido para Mayoría Cualificada (%)
                </label>
                <input
                  type="number"
                  min="50.01"
                  max="100"
                  step="0.01"
                  className="form-input"
                  value={defaultMajorityPercentage ?? 66.67}
                  onChange={e => setDefaultMajorityPercentage(Number(e.target.value))}
                  required
                />
              </div>
            )}

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                Política de Tratamiento de Abstenciones
              </label>
              <select
                className="form-input"
                value={abstentionPolicy}
                onChange={e => setAbstentionPolicy(Number(e.target.value) as AbstentionPolicy)}
              >
                <option value={AbstentionPolicy.Excluded}>Excluidas del cómputo (Recomendado MVP)</option>
                <option value={AbstentionPolicy.IncludedInDenominator}>Incluidas en el denominador total</option>
                <option value={AbstentionPolicy.IncludedAsAgainst}>Contadas como votos En contra</option>
              </select>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end', marginTop: '8px' }}>
            <button type="button" onClick={onClose} className="btn btn-secondary">
              Cancelar
            </button>
            <button type="submit" disabled={saving} className="btn btn-primary">
              {saving ? 'Guardando...' : 'Guardar Ajustes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
