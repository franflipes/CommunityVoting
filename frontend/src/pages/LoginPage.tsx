import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../api/client';
import { AuthResponse, UserRole } from '../types';
import { ThemeToggle } from '../components/ThemeToggle';
import { Vote, LogIn, UserPlus, Shield, UserCheck, KeyRound, ArrowLeft } from 'lucide-react';

type AuthMode = 'login' | 'register' | 'reset';

export const LoginPage: React.FC = () => {
  const [authMode, setAuthMode] = useState<AuthMode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [name, setName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [role, setRole] = useState<UserRole>(UserRole.CommunityMember);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleAuth = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);
    try {
      if (authMode === 'register') {
        const res = await apiClient.post<AuthResponse>('/auth/register', {
          name,
          lastName,
          email,
          password,
          phoneNumber,
          role: Number(role)
        });
        login(res.data);
        navigate('/communities');
      } else if (authMode === 'login') {
        const res = await apiClient.post<AuthResponse>('/auth/login', { email, password });
        login(res.data);
        navigate('/communities');
      } else if (authMode === 'reset') {
        const res = await apiClient.post<{ message: string }>('/auth/reset-password', {
          email,
          newPassword
        });
        setSuccess(res.data.message || 'Contraseña restablecida correctamente. Ahora puedes iniciar sesión.');
        setPassword('');
        setNewPassword('');
        setAuthMode('login');
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error de autenticación. Comprueba tus datos.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = async (presetEmail: string, presetPass: string) => {
    setError('');
    setSuccess('');
    setLoading(true);
    try {
      const res = await apiClient.post<AuthResponse>('/auth/login', { email: presetEmail, password: presetPass });
      login(res.data);
      navigate('/communities');
    } catch (err: any) {
      setError('Error en inicio de sesión rápido demo.');
    } finally {
      setLoading(false);
    }
  };

  const getTitle = () => {
    if (authMode === 'register') return 'Crear Cuenta';
    if (authMode === 'reset') return 'Restablecer Contraseña';
    return 'Iniciar Sesión';
  };

  const getSubtitle = () => {
    if (authMode === 'register') return 'Regístrate para participar en las votaciones';
    if (authMode === 'reset') return 'Introduce tu correo electrónico y tu nueva contraseña';
    return 'Accede a la plataforma de votación de tu comunidad';
  };

  return (
    <div style={{ maxWidth: '440px', margin: '40px auto 0' }}>
      <div className="glass-panel" style={{ padding: '36px', position: 'relative' }}>
        <div style={{ position: 'absolute', top: '16px', right: '16px' }}>
          <ThemeToggle />
        </div>
        <div style={{ textAlign: 'center', marginBottom: '28px' }}>
          <div style={{
            width: '56px',
            height: '56px',
            borderRadius: '16px',
            background: 'linear-gradient(135deg, #6366f1 0%, #06b6d4 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 16px',
            boxShadow: '0 0 25px rgba(99, 102, 241, 0.5)'
          }}>
            <Vote size={32} color="white" />
          </div>
          <h2 style={{ fontSize: '1.6rem', fontWeight: '800' }}>
            {getTitle()}
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginTop: '4px' }}>
            {getSubtitle()}
          </p>
        </div>

        {error && <div style={{ padding: '12px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', border: '1px solid rgba(239, 68, 68, 0.3)', marginBottom: '20px', fontSize: '0.85rem' }}>{error}</div>}
        {success && <div style={{ padding: '12px', borderRadius: '8px', background: 'rgba(34, 197, 94, 0.15)', color: '#4ade80', border: '1px solid rgba(34, 197, 94, 0.3)', marginBottom: '20px', fontSize: '0.85rem' }}>{success}</div>}

        <form onSubmit={handleAuth}>
          {authMode === 'register' && (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                <div className="form-group">
                  <label className="form-label">Nombre</label>
                  <input className="form-input" type="text" placeholder="Carlos" value={name} onChange={e => setName(e.target.value)} required />
                </div>
                <div className="form-group">
                  <label className="form-label">Apellidos</label>
                  <input className="form-input" type="text" placeholder="Administrador" value={lastName} onChange={e => setLastName(e.target.value)} required />
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Teléfono</label>
                <input className="form-input" type="text" placeholder="+34 600000000" value={phoneNumber} onChange={e => setPhoneNumber(e.target.value)} />
              </div>
              <div className="form-group">
                <label className="form-label">Rol Inicial</label>
                <select className="form-select" value={role} onChange={e => setRole(Number(e.target.value))}>
                  <option value={UserRole.CommunityMember}>Votante / Miembro</option>
                  <option value={UserRole.CommunityAdmin}>Administrador de Comunidad</option>
                </select>
              </div>
            </>
          )}

          <div className="form-group">
            <label className="form-label">Correo Electrónico</label>
            <input className="form-input" type="email" placeholder="usuario@comunidad.com" value={email} onChange={e => setEmail(e.target.value)} required />
          </div>

          {authMode !== 'reset' && (
            <div className="form-group" style={{ marginBottom: authMode === 'login' ? '4px' : undefined }}>
              <label className="form-label">Contraseña</label>
              <input className="form-input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required />
            </div>
          )}

          {authMode === 'login' && (
            <div style={{ textAlign: 'right', marginBottom: '16px' }}>
              <button
                type="button"
                onClick={() => {
                  setError('');
                  setSuccess('');
                  setAuthMode('reset');
                }}
                style={{ background: 'none', border: 'none', color: 'var(--secondary, #818cf8)', cursor: 'pointer', fontSize: '0.82rem', textDecoration: 'underline' }}
              >
                ¿Olvidaste tu contraseña?
              </button>
            </div>
          )}

          {authMode === 'reset' && (
            <div className="form-group">
              <label className="form-label">Nueva Contraseña</label>
              <input className="form-input" type="password" placeholder="Mínimo 6 caracteres" value={newPassword} onChange={e => setNewPassword(e.target.value)} required minLength={6} />
            </div>
          )}

          <button type="submit" disabled={loading} className="btn btn-primary" style={{ width: '100%', marginTop: authMode === 'login' ? '4px' : '12px' }}>
            {authMode === 'register' ? <UserPlus size={18} /> : authMode === 'reset' ? <KeyRound size={18} /> : <LogIn size={18} />}
            {loading ? 'Procesando...' : authMode === 'register' ? 'Registrarse' : authMode === 'reset' ? 'Guardar Nueva Contraseña' : 'Entrar'}
          </button>
        </form>

        <div style={{ textAlign: 'center', marginTop: '20px' }}>
          {authMode === 'reset' ? (
            <button
              onClick={() => {
                setError('');
                setSuccess('');
                setAuthMode('login');
              }}
              style={{ background: 'none', border: 'none', color: 'var(--secondary)', cursor: 'pointer', fontSize: '0.85rem', display: 'inline-flex', alignItems: 'center', gap: '6px' }}
            >
              <ArrowLeft size={16} /> Volver a Iniciar Sesión
            </button>
          ) : (
            <button
              onClick={() => {
                setError('');
                setSuccess('');
                setAuthMode(authMode === 'login' ? 'register' : 'login');
              }}
              style={{ background: 'none', border: 'none', color: 'var(--secondary)', cursor: 'pointer', fontSize: '0.85rem', textDecoration: 'underline' }}
            >
              {authMode === 'register' ? '¿Ya tienes cuenta? Inicia Sesión' : '¿No tienes cuenta? Regístrate aquí'}
            </button>
          )}
        </div>

        {/* Demo Quick Access */}
        {authMode === 'login' && (
          <div style={{ marginTop: '28px', paddingTop: '20px', borderTop: '1px solid var(--border-color)' }}>
            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', fontWeight: '600', textTransform: 'uppercase', letterSpacing: '0.05em', display: 'block', marginBottom: '12px', textAlign: 'center' }}>
              Acceso Rápido Demo
            </span>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
              <button onClick={() => handleQuickLogin('laura@comunidad.com', 'Admin123!')} className="btn btn-secondary btn-sm" style={{ fontSize: '0.75rem' }}>
                <Shield size={14} color="#818cf8" /> Laura (Admin)
              </button>
              <button onClick={() => handleQuickLogin('juan@comunidad.com', 'Voter123!')} className="btn btn-secondary btn-sm" style={{ fontSize: '0.75rem' }}>
                <UserCheck size={14} color="#34d399" /> Juan (Votante)
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

