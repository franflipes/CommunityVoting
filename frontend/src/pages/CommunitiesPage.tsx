import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { apiClient } from '../api/client';
import { useAuth } from '../contexts/AuthContext';
import { Community } from '../types';
import { CreateCommunityModal } from '../components/CreateCommunityModal';
import { Building2, Plus, Users, ArrowRight, ShieldCheck } from 'lucide-react';

export const CommunitiesPage: React.FC = () => {
  const { user, isAdmin } = useAuth();
  const [communities, setCommunities] = useState<Community[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    fetchCommunities();
  }, []);

  const fetchCommunities = async () => {
    try {
      const res = await apiClient.get<Community[]>('/communities');
      setCommunities(res.data);
    } catch (err) {
      console.error('Error fetching communities', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '28px', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <h1 style={{ fontSize: '1.8rem', fontWeight: '800' }}>Comunidades Registradas</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.92rem', marginTop: '4px' }}>
            Accede a las juntas y salas de votación privadas de tu comunidad.
          </p>
        </div>

        {isAdmin && (
          <button onClick={() => setIsModalOpen(true)} className="btn btn-primary">
            <Plus size={18} /> Nueva Comunidad
          </button>
        )}
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-secondary)' }}>Cargando comunidades...</div>
      ) : communities.length === 0 ? (
        <div className="glass-panel" style={{ padding: '48px', textAlign: 'center', color: 'var(--text-secondary)' }}>
          <Building2 size={48} style={{ opacity: 0.4, marginBottom: '16px' }} />
          <h3 style={{ fontSize: '1.2rem', color: 'var(--text-primary)', marginBottom: '8px' }}>No hay comunidades registradas</h3>
          <p style={{ fontSize: '0.9rem' }}>Pide a tu administrador que te agregue a una comunidad o crea una si tienes permisos de administración.</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '24px' }}>
          {communities.map((c) => (
            <div key={c.id} className="glass-panel glass-panel-hover" style={{ padding: '24px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '12px' }}>
                  <div style={{ width: '42px', height: '42px', borderRadius: '12px', background: 'hsla(250, 84%, 54%, 0.15)', display: 'flex', alignItems: 'center', justifyContent: 'center', border: '1px solid var(--border-glass)' }}>
                    <Building2 size={22} color="var(--accent)" />
                  </div>
                  <span className="badge badge-privacy">
                    <ShieldCheck size={12} /> Censo Auditado
                  </span>
                </div>

                <h3 style={{ fontSize: '1.25rem', marginBottom: '6px' }}>{c.name}</h3>
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem', marginBottom: '16px' }}>{c.address}</p>
              </div>

              <div>
                <div style={{ display: 'flex', gap: '16px', fontSize: '0.82rem', color: 'var(--text-secondary)', padding: '12px 0', borderTop: '1px solid var(--border-glass)', marginBottom: '16px' }}>
                  <span style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                    <Users size={14} color="var(--accent)" /> {c.membersCount ?? c.memberCount ?? 0} Miembros
                  </span>
                </div>

                <Link to={`/communities/${c.id}`} className="btn btn-secondary" style={{ width: '100%', justifyContent: 'center' }}>
                  Ver Juntas y Votaciones <ArrowRight size={16} />
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      <CreateCommunityModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSuccess={() => fetchCommunities()}
      />
    </div>
  );
};
