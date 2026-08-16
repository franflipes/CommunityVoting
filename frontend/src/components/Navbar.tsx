import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ThemeToggle } from './ThemeToggle';
import { Vote, LogOut, User as UserIcon, Building2, Shield, Settings } from 'lucide-react';
import { UserRole } from '../types';

export const Navbar: React.FC = () => {
  const { user, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header style={{
      borderBottom: '1px solid var(--border-glass)',
      background: 'var(--bg-surface)',
      backdropFilter: 'var(--blur-glass)',
      WebkitBackdropFilter: 'var(--blur-glass)',
      position: 'sticky',
      top: 0,
      zIndex: 50
    }}>
      <div style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '0 24px',
        height: '72px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
      }}>
        <Link to="/" style={{ textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '14px' }}>
          <div style={{
            width: '42px',
            height: '42px',
            borderRadius: '14px',
            background: 'linear-gradient(135deg, var(--accent) 0%, hsl(190, 90%, 50%) 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 0 20px var(--accent-glow)'
          }}>
            <Vote size={24} color="white" />
          </div>
          <div>
            <h2 style={{ fontSize: '1.3rem', fontWeight: '800' }} className="gradient-text">CommunityVoting</h2>
            <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', display: 'block', marginTop: '-2px', fontWeight: '500' }}>
              SaaS Votaciones Privadas
            </span>
          </div>
        </Link>

        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          <ThemeToggle />

          {user && (
            <>
              <Link to="/communities" className="btn btn-secondary btn-sm">
                <Building2 size={16} /> Comunidades
              </Link>

              {isAdmin && (
                <Link to="/admin/settings" className="btn btn-secondary btn-sm">
                  <Settings size={16} /> Configuración
                </Link>
              )}

              <div style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '6px 14px', borderRadius: '12px', background: 'var(--bg-surface)', border: '1px solid var(--border-glass)' }}>
                <div style={{ width: '32px', height: '32px', borderRadius: '50%', background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <UserIcon size={16} color="white" />
                </div>
                <div style={{ fontSize: '0.85rem' }}>
                  <div style={{ fontWeight: '600', color: 'var(--text-primary)' }}>{user.name} {user.lastName}</div>
                  <div style={{ fontSize: '0.74rem', color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                    {isAdmin && <Shield size={12} color="var(--accent)" />}
                    {user.role === UserRole.GlobalAdmin ? 'Admin Global' : user.role === UserRole.CommunityAdmin ? 'Admin Comunidad' : 'Votante'}
                  </div>
                </div>
              </div>

              <button onClick={handleLogout} className="btn btn-secondary btn-sm" title="Cerrar sesión">
                <LogOut size={16} />
              </button>
            </>
          )}
        </div>
      </div>
    </header>
  );
};
