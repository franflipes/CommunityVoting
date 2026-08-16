import React from 'react';
import { ShieldCheck, Radio, Lock } from 'lucide-react';

interface PrivacyHeaderProps {
  communityName: string;
  meetingTitle?: string;
  isConnected: boolean;
}

export const PrivacyHeader: React.FC<PrivacyHeaderProps> = ({ communityName, meetingTitle, isConnected }) => {
  return (
    <div className="glass-panel" style={{ padding: '20px 24px', marginBottom: '24px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '12px' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '4px' }}>
            <span className="badge badge-privacy">
              <Lock size={12} /> Acceso Privado / Token Verificado
            </span>
            <span className={`badge ${isConnected ? 'badge-success' : 'badge-warning'}`}>
              <Radio size={12} className={isConnected ? 'pulse' : ''} />
              {isConnected ? 'SignalR Conectado / Tiempo Real' : 'Conectando...'}
            </span>
          </div>

          <h2 style={{ fontSize: '1.4rem', fontWeight: '800', marginTop: '6px' }}>
            {communityName}
          </h2>
          {meetingTitle && (
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginTop: '2px' }}>
              {meetingTitle}
            </p>
          )}
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '8px 14px', borderRadius: '12px', background: 'hsla(250, 84%, 54%, 0.1)', border: '1px solid var(--border-glass)' }}>
          <ShieldCheck size={20} color="var(--accent)" />
          <span style={{ fontSize: '0.82rem', fontWeight: '600', color: 'var(--text-primary)' }}>
            Votación Encriptada y Censo Auditado
          </span>
        </div>
      </div>
    </div>
  );
};
