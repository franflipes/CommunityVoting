import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { apiClient } from '../api/client';
import { useAuth } from '../contexts/AuthContext';
import { VerifyInvitationResponse, AuthResponse } from '../types';
import { Vote, UserCheck, AlertTriangle, CheckCircle2, Building2, ArrowRight, ShieldCheck } from 'lucide-react';

export const JoinCommunityPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const navigate = useNavigate();
  const { login } = useAuth();

  const [verifyStatus, setVerifyStatus] = useState<VerifyInvitationResponse | null>(null);
  const [verifying, setVerifying] = useState(true);

  // Form inputs
  const [name, setName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    if (token) {
      verifyInvitationToken(token);
    } else {
      setVerifying(false);
      setVerifyStatus({
        isValid: false,
        errorMessage: 'No se ha especificado ningún token de invitación en el enlace.',
      });
    }
  }, [token]);

  const verifyInvitationToken = async (inviteToken: string) => {
    setVerifying(true);
    try {
      const res = await apiClient.get<VerifyInvitationResponse>(`/invitations/verify/${inviteToken}`);
      setVerifyStatus(res.data);
    } catch (err: any) {
      setVerifyStatus({
        isValid: false,
        errorMessage: 'Error al conectar con el servidor para verificar la invitación.',
      });
    } finally {
      setVerifying(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token || !verifyStatus?.isValid) return;

    setLoading(true);
    setErrorMessage('');
    try {
      const res = await apiClient.post<AuthResponse>('/auth/register-with-invitation', {
        token,
        name,
        lastName,
        email,
        password,
        phoneNumber,
      });

      // Save token and login
      localStorage.setItem('auth_token', res.data.token);
      localStorage.setItem('auth_user', JSON.stringify(res.data.user));
      
      // Update auth context state by reloading or navigating
      window.location.href = res.data.user ? `/communities/${verifyStatus.communityId}` : '/communities';
    } catch (err: any) {
      setErrorMessage(err.response?.data?.error || 'Error completando el registro en la comunidad.');
    } finally {
      setLoading(false);
    }
  };

  if (verifying) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <div className="glass-panel" style={{ padding: '40px', textAlign: 'center', maxWidth: '420px', width: '90%' }}>
          <div style={{ width: '48px', height: '48px', borderRadius: '50%', background: 'hsla(250, 84%, 54%, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 16px' }}>
            <Vote size={24} color="var(--accent)" />
          </div>
          <h3 style={{ fontSize: '1.2rem', marginBottom: '8px' }}>Verificando Enlace de Invitación...</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem' }}>Comprobando validez legal y seguridad del token comunitaria.</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '32px 24px' }}>
      <div className="glass-panel" style={{ padding: '36px', maxWidth: '480px', width: '100%' }}>
        
        {/* Header Branding */}
        <div style={{ textAlign: 'center', marginBottom: '28px' }}>
          <div style={{ width: '54px', height: '54px', borderRadius: '16px', background: 'linear-gradient(135deg, var(--accent) 0%, hsl(190, 90%, 50%) 100%)', display: 'flex', alignItems: 'center', justifyContent: 'center', margin: '0 auto 14px', boxShadow: '0 0 24px var(--accent-glow)' }}>
            <Vote size={30} color="white" />
          </div>
          <h2 style={{ fontSize: '1.6rem', fontWeight: '800' }} className="gradient-text">CommunityVoting</h2>
          <span style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', display: 'block', marginTop: '2px' }}>
            Unirse a la Comunidad por Invitación
          </span>
        </div>

        {/* Verification Status Banner */}
        {!verifyStatus?.isValid ? (
          <div style={{ padding: '24px', borderRadius: '14px', background: 'hsla(0, 84%, 50%, 0.12)', border: '1px solid hsl(0, 84%, 50%)', textAlign: 'center' }}>
            <AlertTriangle size={32} color="hsl(0, 84%, 60%)" style={{ marginBottom: '12px' }} />
            <h3 style={{ fontSize: '1.15rem', color: 'white', marginBottom: '8px' }}>Enlace Inválido o Expirado</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem', marginBottom: '20px' }}>
              {verifyStatus?.errorMessage || 'El enlace de invitación especificado no se encuentra registrado en el sistema.'}
            </p>
            <Link to="/login" className="btn btn-secondary btn-sm" style={{ display: 'inline-flex' }}>
              Ir al Inicio de Sesión
            </Link>
          </div>
        ) : (
          <div>
            <div style={{ padding: '16px', borderRadius: '12px', background: 'hsla(142, 71%, 45%, 0.12)', border: '1px solid hsl(142, 71%, 45%)', marginBottom: '24px', display: 'flex', alignItems: 'center', gap: '12px' }}>
              <Building2 size={24} color="hsl(142, 71%, 60%)" />
              <div>
                <span style={{ fontSize: '0.75rem', color: 'hsl(142, 80%, 75%)', textTransform: 'uppercase', fontWeight: '700' }}>Invitación Verificada ✓</span>
                <div style={{ fontSize: '1.05rem', fontWeight: '700', color: 'white' }}>{verifyStatus.communityName}</div>
              </div>
            </div>

            {errorMessage && (
              <div style={{ padding: '12px 16px', borderRadius: '10px', background: 'hsla(0, 84%, 50%, 0.15)', color: 'hsl(0, 84%, 70%)', border: '1px solid hsla(0, 84%, 50%, 0.3)', marginBottom: '20px', fontSize: '0.88rem' }}>
                {errorMessage}
              </div>
            )}

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                    Nombre
                  </label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Tu nombre"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                    Apellidos
                  </label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Tus apellidos"
                    value={lastName}
                    onChange={e => setLastName(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                  Correo Electrónico
                </label>
                <input
                  type="email"
                  className="form-input"
                  placeholder="ejemplo@correo.com"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                  Teléfono de Contacto (Opcional)
                </label>
                <input
                  type="tel"
                  className="form-input"
                  placeholder="+34 600 000 000"
                  value={phoneNumber}
                  onChange={e => setPhoneNumber(e.target.value)}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                  Contraseña de Acceso
                </label>
                <input
                  type="password"
                  className="form-input"
                  placeholder="Mínimo 6 caracteres"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  required
                />
              </div>

              <button type="submit" disabled={loading} className="btn btn-primary" style={{ width: '100%', marginTop: '8px', justifyContent: 'center' }}>
                {loading ? 'Registrando en Comunidad...' : 'Completar Registro y Entrar'} <ArrowRight size={18} />
              </button>
            </form>

            <div style={{ textAlign: 'center', marginTop: '20px', fontSize: '0.84rem', color: 'var(--text-secondary)' }}>
              ¿Ya tienes cuenta? <Link to="/login" style={{ color: 'var(--accent)', textDecoration: 'none', fontWeight: '600' }}>Inicia Sesión</Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
