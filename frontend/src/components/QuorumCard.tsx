import React from 'react';
import { Users, CheckCircle2, AlertTriangle, UserCheck } from 'lucide-react';
import { QuorumStatus } from '../types';

interface QuorumCardProps {
  status: QuorumStatus | null;
  isPresent?: boolean;
  onRecordAttendance?: () => void;
  isAdmin?: boolean;
  onOpenSettings?: () => void;
  onOpenAttendanceList?: () => void;
}

export const QuorumCard: React.FC<QuorumCardProps> = ({
  status,
  isPresent,
  onRecordAttendance,
  isAdmin,
  onOpenSettings,
  onOpenAttendanceList,
}) => {
  if (!status) return null;

  const percentagePresent = status.eligibleMembers > 0
    ? (status.presentMembers / status.eligibleMembers) * 100
    : 0;

  return (
    <div className="glass-panel" style={{ padding: '24px', marginBottom: '28px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '14px', flexWrap: 'wrap', gap: '12px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Users size={20} color="var(--accent)" />
          <h3 style={{ fontSize: '1.1rem', margin: 0 }}>Quórum y Asistencia a la Reunión</h3>
        </div>

        <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
          {onOpenAttendanceList && (
            <button onClick={onOpenAttendanceList} className="btn btn-secondary btn-sm" style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <UserCheck size={14} color="hsl(142, 71%, 65%)" /> Ver Asistentes ({status.presentMembers})
            </button>
          )}

          <span
            className={`badge ${status.quorumReached ? 'badge-success' : 'badge-warning'}`}
            style={{ display: 'flex', alignItems: 'center', gap: '6px' }}
          >
            {status.quorumReached ? (
              <>
                <CheckCircle2 size={14} /> Quórum Alcanzado ({status.quorumPercentage}%)
              </>
            ) : (
              <>
                <AlertTriangle size={14} /> Quórum Incompleto (Requerido: {status.quorumPercentage}%)
              </>
            )}
          </span>

          {isAdmin && onOpenSettings && (
            <button onClick={onOpenSettings} className="btn btn-secondary btn-sm">
              Configurar Reglas
            </button>
          )}
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '16px', marginBottom: '16px' }}>
        <div>
          <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', display: 'block' }}>Miembros Con Derecho a Voto</span>
          <strong style={{ fontSize: '1.2rem', color: 'var(--text-primary)' }}>{status.eligibleMembers}</strong>
        </div>

        <div>
          <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', display: 'block' }}>Asistentes Presentes</span>
          <strong style={{ fontSize: '1.2rem', color: 'hsl(142, 71%, 65%)' }}>{status.presentMembers}</strong>
        </div>

        <div>
          <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', display: 'block' }}>Asistencia Mínima Requerida</span>
          <strong style={{ fontSize: '1.2rem', color: 'var(--accent)' }}>{status.quorumRequired}</strong>
        </div>

        {onRecordAttendance && (
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <button
              onClick={onRecordAttendance}
              className={`btn ${isPresent ? 'btn-success' : 'btn-primary'} btn-sm`}
              style={{ width: '100%' }}
            >
              <UserCheck size={16} /> {isPresent ? 'Asistencia Confirmada ✓' : 'Marcar Mi Asistencia'}
            </button>
          </div>
        )}
      </div>

      {/* Progress Bar */}
      <div className="progress-bar-bg">
        <div
          className="progress-bar-fill"
          style={{
            width: `${Math.min(percentagePresent, 100)}%`,
            background: status.quorumReached
              ? 'linear-gradient(90deg, hsl(142, 71%, 45%), hsl(142, 90%, 55%))'
              : 'linear-gradient(90deg, hsl(38, 92%, 50%), hsl(38, 100%, 60%))',
          }}
        ></div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.78rem', color: 'var(--text-secondary)', marginTop: '8px' }}>
        <span>Asistencia Actual: {percentagePresent.toFixed(1)}%</span>
        <span>{status.requireQuorumForVoting ? 'Quórum Obligatorio para Votar' : 'Quórum Opcional'}</span>
      </div>
    </div>
  );
};
