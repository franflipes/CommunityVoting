import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { apiClient } from '../api/client';
import { AuthResponse, UserRole } from '../types';
import { ThemeToggle } from '../components/ThemeToggle';
import { Vote, LogIn, UserPlus, Shield, UserCheck } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const [isRegister, setIsRegister] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [role, setRole] = useState<UserRole>(UserRole.CommunityMember);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleAuth = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      if (isRegister) {
        const res = await apiClient.post<AuthResponse>('/auth/register', {
          name,
          lastName,
          email,
          password,
          phoneNumber,
          role: Number(role)
        });
        login(res.data);
      } else {
        const res = await apiClient.post<AuthResponse>('/auth/login', { email, password });
        login(res.data);
      }
      navigate('/communities');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error de autenticación. Comprueba tus datos.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = async (presetEmail: string, presetPass: string) => {
    setError('');
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
            {isRegister ? 'Crear Cuenta' : 'Iniciar Sesión'}
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginTop: '4px' }}>
            {isRegister ? 'Regístrate para participar en las votaciones' : 'Accede a la plataforma de votación de tu comunidad'}
          </p>
        </div>

        {error && <div style={{ padding: '12px', borderRadius: '8px', background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', border: '1px solid rgba(239, 68, 68, 0.3)', marginBottom: '20px', fontSize: '0.85rem' }}>{error}</div>}

        <form onSubmit={handleAuth}>
          {isRegister && (
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

          <div className="form-group">
            <label className="form-label">Contraseña</label>
            <input className="form-input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} required />
          </div>

          <button type="submit" disabled={loading} className="btn btn-primary" style={{ width: '100%', marginTop: '12px' }}>
            {isRegister ? <UserPlus size={18} /> : <LogIn size={18} />}
            {loading ? 'Procesando...' : isRegister ? 'Registrarse' : 'Entrar'}
          </button>
        </form>

        <div style={{ textAlign: 'center', marginTop: '20px' }}>
          <button
            onClick={() => setIsRegister(!isRegister)}
            style={{ background: 'none', border: 'none', color: 'var(--secondary)', cursor: 'pointer', fontSize: '0.85rem', textDecoration: 'underline' }}
          >
            {isRegister ? '¿Ya tienes cuenta? Inicia Sesión' : '¿No tienes cuenta? Regístrate aquí'}
          </button>
        </div>

        {/* Demo Quick Access */}
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
      </div>
    </div>
  );
};
