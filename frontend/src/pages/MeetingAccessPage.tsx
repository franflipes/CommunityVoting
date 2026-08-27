import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../api/client';
import { MeetingAccessAuthResponse } from '../types';
import { ThemeToggle } from '../components/ThemeToggle';
import { Vote, KeyRound, LogIn, AlertCircle } from 'lucide-react';

export const MeetingAccessPage: React.FC = () => {
  const { token } = useParams<{ token: string }>();
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleAccess = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!token) {
      setError('El enlace de acceso no contiene un token válido.');
      return;
    }

    if (!code || code.trim().length < 4) {
      setError('Por favor, introduce tu código de acceso.');
      return;
    }

    setLoading(true);
    try {
      const res = await apiClient.post<MeetingAccessAuthResponse>('/auth/meeting-access', {
        token,
        code: code.trim()
      });

      // Login user with returned JWT token & profile
      login({
        token: res.data.accessToken,
        user: res.data.user
      });

      // Navigate to the target meeting
      navigate(res.data.redirectUrl || '/communities');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al validar la credencial de acceso. Comprueba el código.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: '440px', margin: '40px auto 0' }}>
      <div className="glass-panel" style={{ padding: '36px', position: 'relative' }}>
        <div style={{ position: 'absolute', top: '16px', right: '16px' }}>
          <ThemeToggle />
        </div>

        <div style={{ textAlign: 'center', marginBottom: '28px' }}>
          <div style={{
            width: '60px',
            height: '60px',
            borderRadius: '16px',
            background: 'linear-gradient(135deg, #6366f1 0%, #06b6d4 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 16px',
            boxShadow: '0 0 25px rgba(99, 102, 241, 0.5)'
          }}>
            <Vote size={34} color="white" />
          </div>

          <h2 style={{ fontSize: '1.6rem', fontWeight: '800' }}>
            Acceso a Reunión
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginTop: '6px', lineHeight: '1.4' }}>
            Introduce el código de acceso de 6 dígitos que has recibido junto a tu enlace de reunión.
          </p>
        </div>

        {error && (
          <div style={{
            padding: '12px 14px',
            borderRadius: '10px',
            background: 'rgba(239, 68, 68, 0.15)',
            color: '#fca5a5',
            border: '1px solid rgba(239, 68, 68, 0.3)',
            marginBottom: '20px',
            fontSize: '0.85rem',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <AlertCircle size={18} style={{ flexShrink: 0 }} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleAccess}>
          <div className="form-group" style={{ marginBottom: '20px' }}>
            <label className="form-label" style={{ fontWeight: '600', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <KeyRound size={16} color="var(--primary)" />
              Código de Acceso
            </label>
            <input
              className="form-input"
              type="text"
              placeholder="Ej. 482913"
              value={code}
              onChange={e => setCode(e.target.value)}
              required
              maxLength={10}
              style={{
                fontSize: '1.25rem',
                letterSpacing: '0.15em',
                textAlign: 'center',
                fontWeight: '700',
                padding: '12px'
              }}
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="btn btn-primary"
            style={{ width: '100%', padding: '14px', fontSize: '1rem', fontWeight: '700' }}
          >
            <LogIn size={20} />
            {loading ? 'Validando Acceso...' : 'Entrar a la Reunión'}
          </button>
        </form>

        <div style={{ textAlign: 'center', marginTop: '24px', paddingTop: '16px', borderTop: '1px solid var(--border-color)' }}>
          <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
            ¿Eres administrador o prefieres usar tu contraseña habitual?{' '}
            <button
              onClick={() => navigate('/login')}
              style={{ background: 'none', border: 'none', color: 'var(--secondary)', cursor: 'pointer', textDecoration: 'underline' }}
            >
              Iniciar sesión aquí
            </button>
          </p>
        </div>
      </div>
    </div>
  );
};
