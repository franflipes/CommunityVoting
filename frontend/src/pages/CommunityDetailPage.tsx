import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { apiClient } from '../api/client';
import { useAuth } from '../contexts/AuthContext';
import { Community, Meeting, CommunityMember, UserRole } from '../types';
import { CreateMeetingModal } from '../components/CreateMeetingModal';
import {
  Building2,
  Calendar,
  Users,
  Plus,
  ChevronRight,
  ArrowRight,
  Shield,
  UserCheck,
  MapPin,
  Clock,
  Settings,
} from 'lucide-react';

export const CommunityDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { isAdmin } = useAuth();

  const [community, setCommunity] = useState<Community | null>(null);
  const [meetings, setMeetings] = useState<Meeting[]>([]);
  const [members, setMembers] = useState<CommunityMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [isMeetingModalOpen, setIsMeetingModalOpen] = useState(false);

  useEffect(() => {
    if (id) {
      fetchCommunityData();
    }
  }, [id]);

  const fetchCommunityData = async () => {
    try {
      const [commRes, meetRes, membRes] = await Promise.all([
        apiClient.get<Community>(`/communities/${id}`),
        apiClient.get<Meeting[]>(`/meetings/community/${id}`),
        apiClient.get<CommunityMember[]>(`/communities/${id}/members`),
      ]);

      setCommunity(commRes.data);
      setMeetings(meetRes.data);
      setMembers(membRes.data);
    } catch (err) {
      console.error('Error fetching community details', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-secondary)' }}>Cargando información de la comunidad...</div>;
  }

  if (!community) {
    return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--danger)' }}>Comunidad no encontrada.</div>;
  }

  return (
    <div>
      {/* Breadcrumb */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
        <Link to="/communities" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>Comunidades</Link>
        <ChevronRight size={14} />
        <span style={{ color: 'var(--text-primary)', fontWeight: '600' }}>{community.name}</span>
      </div>

      {/* Header Banner */}
      <div className="glass-panel" style={{ padding: '28px', marginBottom: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '16px' }}>
          <div>
            <span className="badge badge-privacy" style={{ marginBottom: '8px' }}>
              CIF: {community.cif || 'No especificado'}
            </span>
            <h1 style={{ fontSize: '1.8rem', marginTop: '6px' }}>{community.name}</h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.92rem', marginTop: '4px' }}>
              <MapPin size={14} style={{ verticalAlign: 'middle', marginRight: '4px' }} />
              {community.address}
            </p>
          </div>

          {isAdmin && (
            <div style={{ display: 'flex', gap: '10px' }}>
              <Link to={`/communities/${community.id}/settings`} className="btn btn-secondary">
                <Settings size={18} /> Configurar Comunidad
              </Link>
              <button onClick={() => setIsMeetingModalOpen(true)} className="btn btn-primary">
                <Plus size={18} /> Convocar Nueva Junta
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Grid: Meetings vs Censo de Miembros */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '28px' }}>
        
        {/* Meetings List */}
        <div>
          <h2 style={{ fontSize: '1.4rem', marginBottom: '18px', display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Calendar color="var(--accent)" /> Juntas y Reuniones Convocadas
          </h2>

          {meetings.length === 0 ? (
            <div className="glass-panel" style={{ padding: '36px', textAlign: 'center', color: 'var(--text-secondary)' }}>
              <Calendar size={36} style={{ opacity: 0.4, marginBottom: '12px' }} />
              <p style={{ fontSize: '0.95rem' }}>No hay reuniones o juntas convocadas para esta comunidad.</p>
              {isAdmin && (
                <button onClick={() => setIsMeetingModalOpen(true)} className="btn btn-secondary btn-sm" style={{ marginTop: '16px' }}>
                  <Plus size={14} /> Convocar Primera Junta
                </button>
              )}
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              {meetings.map((m) => (
                <div key={m.id} className="glass-panel glass-panel-hover" style={{ padding: '20px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
                    <span className="badge badge-privacy">
                      {m.type === 0 ? 'Junta Ordinaria' : 'Junta Extraordinaria'}
                    </span>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                      <Clock size={12} /> {new Date(m.scheduledAt).toLocaleDateString()}
                    </span>
                  </div>

                  <h3 style={{ fontSize: '1.15rem', marginBottom: '6px' }}>{m.title}</h3>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '16px' }}>
                    Lugar: {m.location}
                  </p>

                  <Link to={`/meetings/${m.id}`} className="btn btn-secondary btn-sm" style={{ width: '100%', justifyContent: 'center' }}>
                    Ver Orden del Día y Votaciones <ArrowRight size={14} />
                  </Link>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Members Censo */}
        <div>
          <h2 style={{ fontSize: '1.4rem', marginBottom: '18px', display: 'flex', alignItems: 'center', gap: '10px' }}>
            <Users color="var(--accent)" /> Censo de Miembros ({members.length})
          </h2>

          <div className="glass-panel" style={{ padding: '20px' }}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              {members.map((m) => (
                <div key={m.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 14px', borderRadius: '10px', background: 'var(--bg-surface)', border: '1px solid var(--border-glass)' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                    <div style={{ width: '34px', height: '34px', borderRadius: '50%', background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <UserCheck size={16} color="white" />
                    </div>
                    <div>
                      <div style={{ fontWeight: '600', fontSize: '0.9rem' }}>{m.userName}</div>
                      <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>{m.userEmail}</div>
                    </div>
                  </div>

                  <span className={`badge ${m.memberRole === UserRole.CommunityAdmin ? 'badge-success' : 'badge-privacy'}`} style={{ fontSize: '0.72rem' }}>
                    {m.memberRole === UserRole.CommunityAdmin ? 'Administrador' : 'Votante'}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      <CreateMeetingModal
        communityId={community.id}
        isOpen={isMeetingModalOpen}
        onClose={() => setIsMeetingModalOpen(false)}
        onSuccess={() => fetchCommunityData()}
      />
    </div>
  );
};
