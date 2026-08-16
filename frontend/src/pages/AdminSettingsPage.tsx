import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { apiClient } from '../api/client';
import { useAuth } from '../contexts/AuthContext';
import {
  Community,
  CommunityMember,
  VotingSettings,
  UserRole,
  QuorumType,
  MajorityType,
  AbstentionPolicy,
} from '../types';
import { InvitationManagerModal } from '../components/InvitationManagerModal';
import {
  ShieldCheck,
  Building2,
  Users,
  Settings,
  ChevronRight,
  UserCheck,
  UserX,
  CheckCircle2,
  AlertTriangle,
  Plus,
  Save,
  Lock,
  Link2,
} from 'lucide-react';

export const AdminSettingsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { user, isAdmin } = useAuth();
  const navigate = useNavigate();

  const [communities, setCommunities] = useState<Community[]>([]);
  const [selectedCommunityId, setSelectedCommunityId] = useState<string>(id || '');
  const [community, setCommunity] = useState<Community | null>(null);
  const [members, setMembers] = useState<CommunityMember[]>([]);
  const [settings, setSettings] = useState<VotingSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingSettings, setSavingSettings] = useState(false);
  const [activeTab, setActiveTab] = useState<'voting' | 'members' | 'general'>('voting');
  const [message, setMessage] = useState('');

  // Modals state
  const [isAddMemberOpen, setIsAddMemberOpen] = useState(false);
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [newName, setNewName] = useState('');
  const [newLastName, setNewLastName] = useState('');
  const [newEmail, setNewEmail] = useState('');
  const [newMemberRole, setNewMemberRole] = useState<UserRole>(UserRole.CommunityMember);
  const [addingMember, setAddingMember] = useState(false);

  // Form states for Voting Settings
  const [quorumEnabled, setQuorumEnabled] = useState(true);
  const [quorumPercentage, setQuorumPercentage] = useState(50);
  const [requireQuorumForVoting, setRequireQuorumForVoting] = useState(true);
  const [defaultMajorityType, setDefaultMajorityType] = useState<MajorityType>(MajorityType.SimpleMajority);
  const [defaultMajorityPercentage, setDefaultMajorityPercentage] = useState<number | undefined>(66.67);
  const [abstentionPolicy, setAbstentionPolicy] = useState<AbstentionPolicy>(AbstentionPolicy.Excluded);

  useEffect(() => {
    fetchCommunities();
  }, []);

  useEffect(() => {
    if (selectedCommunityId) {
      fetchCommunityData(selectedCommunityId);
    }
  }, [selectedCommunityId]);

  const fetchCommunities = async () => {
    try {
      const res = await apiClient.get<Community[]>('/communities');
      setCommunities(res.data);
      if (!selectedCommunityId && res.data.length > 0) {
        setSelectedCommunityId(res.data[0].id);
      }
    } catch (err) {
      console.error('Error fetching communities', err);
    }
  };

  const fetchCommunityData = async (communityId: string) => {
    setLoading(true);
    try {
      const [commRes, membRes, setRes] = await Promise.all([
        apiClient.get<Community>(`/communities/${communityId}`),
        apiClient.get<CommunityMember[]>(`/communities/${communityId}/members`),
        apiClient.get<VotingSettings>(`/communities/${communityId}/voting-settings`),
      ]);

      setCommunity(commRes.data);
      setMembers(membRes.data);
      setSettings(setRes.data);

      if (setRes.data) {
        setQuorumEnabled(setRes.data.quorumEnabled);
        setQuorumPercentage(setRes.data.quorumPercentage);
        setRequireQuorumForVoting(setRes.data.requireQuorumForVoting);
        setDefaultMajorityType(setRes.data.defaultMajorityType);
        setDefaultMajorityPercentage(setRes.data.defaultMajorityPercentage ?? 66.67);
        setAbstentionPolicy(setRes.data.abstentionPolicy);
      }
    } catch (err) {
      console.error('Error loading community config', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSaveVotingSettings = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCommunityId) return;

    setSavingSettings(true);
    setMessage('');
    try {
      const payload = {
        quorumEnabled,
        quorumType: QuorumType.PercentageOfEligibleMembers,
        quorumPercentage: Number(quorumPercentage),
        requireQuorumForVoting,
        defaultMajorityType: Number(defaultMajorityType),
        defaultMajorityPercentage: defaultMajorityType === MajorityType.QualifiedMajority ? Number(defaultMajorityPercentage) : undefined,
        abstentionPolicy: Number(abstentionPolicy),
      };

      const res = await apiClient.put<VotingSettings>(`/communities/${selectedCommunityId}/voting-settings`, payload);
      setSettings(res.data);
      setMessage('¡Reglas de votación y quórum actualizadas correctamente!');
    } catch (err) {
      alert('Error guardando reglas de votación');
    } finally {
      setSavingSettings(false);
    }
  };

  const handleToggleVotingRights = async (member: CommunityMember) => {
    if (!selectedCommunityId) return;
    try {
      const newRights = !member.hasVotingRights;
      const res = await apiClient.put<CommunityMember>(`/communities/${selectedCommunityId}/members/${member.userId}/voting-rights`, {
        hasVotingRights: newRights,
        isActive: member.isActive,
      });

      setMembers(prev => prev.map(m => m.id === member.id ? { ...m, hasVotingRights: newRights } : m));
    } catch (err) {
      alert('Error actualizando derecho a voto del miembro');
    }
  };

  const handleToggleActiveStatus = async (member: CommunityMember) => {
    if (!selectedCommunityId) return;
    try {
      const newActive = !member.isActive;
      await apiClient.put<CommunityMember>(`/communities/${selectedCommunityId}/members/${member.userId}/voting-rights`, {
        hasVotingRights: member.hasVotingRights,
        isActive: newActive,
      });

      setMembers(prev => prev.map(m => m.id === member.id ? { ...m, isActive: newActive } : m));
    } catch (err) {
      alert('Error actualizando estado del miembro');
    }
  };

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCommunityId || !newName || !newEmail) return;

    setAddingMember(true);
    try {
      await apiClient.post(`/communities/${selectedCommunityId}/members`, {
        name: newName,
        lastName: newLastName,
        email: newEmail,
        memberRole: Number(newMemberRole),
      });

      setIsAddMemberOpen(false);
      setNewName('');
      setNewLastName('');
      setNewEmail('');
      fetchCommunityData(selectedCommunityId);
      setMessage('Miembro incorporado al censo de la comunidad.');
    } catch (err: any) {
      alert(err.response?.data || 'Error añadiendo miembro');
    } finally {
      setAddingMember(false);
    }
  };

  if (!isAdmin) {
    return (
      <div style={{ textAlign: 'center', padding: '60px', color: 'var(--danger)' }}>
        <Lock size={48} style={{ marginBottom: '16px' }} />
        <h2>Acceso Restringido</h2>
        <p>Solo los Administradores tienen acceso al panel de configuración.</p>
      </div>
    );
  }

  return (
    <div>
      {/* Breadcrumb */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
        <Link to="/communities" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>Comunidades</Link>
        <ChevronRight size={14} />
        <span style={{ color: 'var(--text-primary)', fontWeight: '600' }}>Panel de Configuración del Administrador</span>
      </div>

      {/* Header Panel */}
      <div className="glass-panel" style={{ padding: '28px', marginBottom: '28px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}>
          <div>
            <span className="badge badge-success" style={{ marginBottom: '8px' }}>
              <ShieldCheck size={14} /> Panel de Control de Administración
            </span>
            <h1 style={{ fontSize: '1.8rem', marginTop: '4px', display: 'flex', alignItems: 'center', gap: '10px' }}>
              <Settings color="var(--accent)" /> Configuración General y Reglas Comunitaria
            </h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.92rem', marginTop: '4px' }}>
              Gestión centralizada de reglas de quórum, mayorías de votación y derechos del censo electoral.
            </p>
          </div>

          {/* Community Selector */}
          {communities.length > 0 && (
            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                Seleccionar Comunidad:
              </label>
              <select
                className="form-input"
                style={{ padding: '8px 14px', fontSize: '0.9rem', width: '240px' }}
                value={selectedCommunityId}
                onChange={e => setSelectedCommunityId(e.target.value)}
              >
                {communities.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
          )}
        </div>
      </div>

      {message && (
        <div style={{ padding: '14px 18px', borderRadius: '12px', background: 'hsla(142, 71%, 45%, 0.15)', color: 'hsl(142, 80%, 65%)', border: '1px solid hsla(142, 71%, 45%, 0.35)', marginBottom: '24px', display: 'flex', alignItems: 'center', gap: '10px' }}>
          <CheckCircle2 size={18} /> {message}
        </div>
      )}

      {/* Navigation Tabs */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '24px', borderBottom: '1px solid var(--border-glass)', paddingBottom: '12px' }}>
        <button
          onClick={() => setActiveTab('voting')}
          className={`btn ${activeTab === 'voting' ? 'btn-primary' : 'btn-secondary'}`}
        >
          <ShieldCheck size={16} /> Reglas de Quórum y Mayorías
        </button>
        <button
          onClick={() => setActiveTab('members')}
          className={`btn ${activeTab === 'members' ? 'btn-primary' : 'btn-secondary'}`}
        >
          <Users size={16} /> Censo y Derecho a Voto ({members.length})
        </button>
        <button
          onClick={() => setActiveTab('general')}
          className={`btn ${activeTab === 'general' ? 'btn-primary' : 'btn-secondary'}`}
        >
          <Building2 size={16} /> Información Comunidad
        </button>
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-secondary)' }}>Cargando ajustes de la comunidad...</div>
      ) : !community ? (
        <div style={{ textAlign: 'center', padding: '60px', color: 'var(--danger)' }}>Comunidad no seleccionada.</div>
      ) : (
        <>
          {/* TAB 1: Reglas de Votación & Quórum */}
          {activeTab === 'voting' && (
            <form onSubmit={handleSaveVotingSettings} className="glass-panel" style={{ padding: '28px', maxWidth: '720px' }}>
              <h3 style={{ fontSize: '1.25rem', marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <ShieldCheck color="var(--accent)" /> Reglas Oficiales de Votación para {community.name}
              </h3>

              {/* Section 1: Quorum Settings */}
              <div style={{ padding: '20px', borderRadius: '14px', background: 'var(--bg-surface)', border: '1px solid var(--border-glass)', marginBottom: '20px' }}>
                <h4 style={{ fontSize: '1.05rem', color: 'var(--accent)', marginBottom: '14px' }}>
                  1. Reglas de Quórum de Asistencia
                </h4>

                <label style={{ display: 'flex', alignItems: 'center', gap: '10px', fontSize: '0.92rem', marginBottom: '14px', cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={quorumEnabled}
                    onChange={e => setQuorumEnabled(e.target.checked)}
                  />
                  <strong>Exigir Quórum Mínimo en Juntas</strong>
                </label>

                {quorumEnabled && (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '14px', paddingLeft: '24px', borderLeft: '2px solid var(--accent)' }}>
                    <div>
                      <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                        Porcentaje de Asistencia Mínima Requerido (% sobre censo con voto)
                      </label>
                      <input
                        type="number"
                        min="1"
                        max="100"
                        className="form-input"
                        style={{ width: '100%', maxWidth: '200px' }}
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
                      Bloquear apertura de votaciones si no se alcanza el Quórum requerido
                    </label>
                  </div>
                )}
              </div>

              {/* Section 2: Majority Rules */}
              <div style={{ padding: '20px', borderRadius: '14px', background: 'var(--bg-surface)', border: '1px solid var(--border-glass)', marginBottom: '24px' }}>
                <h4 style={{ fontSize: '1.05rem', color: 'var(--accent)', marginBottom: '14px' }}>
                  2. Reglas de Mayoría para la Aprobación de Propuestas
                </h4>

                <div style={{ marginBottom: '16px' }}>
                  <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                    Tipo de Mayoría por Defecto
                  </label>
                  <select
                    className="form-input"
                    value={defaultMajorityType}
                    onChange={e => setDefaultMajorityType(Number(e.target.value) as MajorityType)}
                  >
                    <option value={MajorityType.SimpleMajority}>Mayoría Simple (Votos A Favor &gt; Votos En Contra)</option>
                    <option value={MajorityType.MajorityOfVotesCast}>Mayoría de Votos Emitidos (&gt; 50% de los votos)</option>
                    <option value={MajorityType.QualifiedMajority}>Mayoría Cualificada (Porcentaje legal superior)</option>
                  </select>
                </div>

                {defaultMajorityType === MajorityType.QualifiedMajority && (
                  <div style={{ marginBottom: '16px' }}>
                    <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                      Porcentaje Requerido para Aprobación Cualificada (%)
                    </label>
                    <input
                      type="number"
                      min="50.01"
                      max="100"
                      step="0.01"
                      className="form-input"
                      style={{ width: '100%', maxWidth: '200px' }}
                      value={defaultMajorityPercentage ?? 66.67}
                      onChange={e => setDefaultMajorityPercentage(Number(e.target.value))}
                      required
                    />
                  </div>
                )}

                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                    Tratamiento de Abstenciones
                  </label>
                  <select
                    className="form-input"
                    value={abstentionPolicy}
                    onChange={e => setAbstentionPolicy(Number(e.target.value) as AbstentionPolicy)}
                  >
                    <option value={AbstentionPolicy.Excluded}>Excluidas del cómputo de mayoría (Estándar)</option>
                    <option value={AbstentionPolicy.IncludedInDenominator}>Incluidas en el total del denominador</option>
                    <option value={AbstentionPolicy.IncludedAsAgainst}>Contabilizadas como votos en contra</option>
                  </select>
                </div>
              </div>

              <button type="submit" disabled={savingSettings} className="btn btn-primary">
                <Save size={16} /> {savingSettings ? 'Guardando Ajustes...' : 'Guardar Configuración de Votación'}
              </button>
            </form>
          )}

          {/* TAB 2: Censo y Derecho a Voto */}
          {activeTab === 'members' && (
            <div className="glass-panel" style={{ padding: '28px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', flexWrap: 'wrap', gap: '12px' }}>
                <div>
                  <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <Users color="var(--accent)" /> Censo Electoral y Gestión de Derechos a Voto
                  </h3>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.88rem', marginTop: '4px' }}>
                    Activa o desactiva los derechos de voto individuales para garantizar la validez legal del quórum.
                  </p>
                </div>

                <div style={{ display: 'flex', gap: '10px' }}>
                  <button onClick={() => setIsInviteModalOpen(true)} className="btn btn-secondary btn-sm">
                    <Link2 size={16} /> Generar Enlace de Invitación
                  </button>
                  <button onClick={() => setIsAddMemberOpen(true)} className="btn btn-primary btn-sm">
                    <Plus size={16} /> Añadir Miembro al Censo
                  </button>
                </div>
              </div>

              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border-glass)', textAlign: 'left', color: 'var(--text-secondary)' }}>
                      <th style={{ padding: '12px 14px' }}>Miembro / Usuario</th>
                      <th style={{ padding: '12px 14px' }}>Email</th>
                      <th style={{ padding: '12px 14px' }}>Rol Comunitaria</th>
                      <th style={{ padding: '12px 14px', textAlign: 'center' }}>Derecho a Voto</th>
                      <th style={{ padding: '12px 14px', textAlign: 'center' }}>Estado Censo</th>
                    </tr>
                  </thead>
                  <tbody>
                    {members.map(m => (
                      <tr key={m.id} style={{ borderBottom: '1px solid var(--border-glass)' }}>
                        <td style={{ padding: '14px', fontWeight: '600' }}>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                            <div style={{ width: '32px', height: '32px', borderRadius: '50%', background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                              <UserCheck size={16} color="white" />
                            </div>
                            {m.userName} {m.userLastName}
                          </div>
                        </td>
                        <td style={{ padding: '14px', color: 'var(--text-secondary)' }}>{m.userEmail}</td>
                        <td style={{ padding: '14px' }}>
                          <span className={`badge ${m.memberRole === UserRole.CommunityAdmin ? 'badge-success' : 'badge-privacy'}`} style={{ fontSize: '0.74rem' }}>
                            {m.memberRole === UserRole.CommunityAdmin ? 'Administrador' : 'Miembro Propietario'}
                          </span>
                        </td>
                        <td style={{ padding: '14px', textAlign: 'center' }}>
                          <button
                            type="button"
                            onClick={() => handleToggleVotingRights(m)}
                            className={`btn ${m.hasVotingRights ? 'btn-success' : 'btn-secondary'} btn-sm`}
                            style={{ padding: '4px 10px', fontSize: '0.78rem' }}
                          >
                            {m.hasVotingRights ? <><UserCheck size={14} /> Con Voto</> : <><UserX size={14} /> Sin Voto</>}
                          </button>
                        </td>
                        <td style={{ padding: '14px', textAlign: 'center' }}>
                          <button
                            type="button"
                            onClick={() => handleToggleActiveStatus(m)}
                            className={`badge ${m.isActive ? 'badge-success' : 'badge-warning'}`}
                            style={{ border: 'none', cursor: 'pointer', fontSize: '0.78rem', padding: '4px 10px' }}
                          >
                            {m.isActive ? 'Activo en Censo' : 'Inactivo'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* TAB 3: Información Comunidad */}
          {activeTab === 'general' && (
            <div className="glass-panel" style={{ padding: '28px', maxWidth: '600px' }}>
              <h3 style={{ fontSize: '1.25rem', marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Building2 color="var(--accent)" /> Datos de la Comunidad
              </h3>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                    Nombre de la Comunidad
                  </label>
                  <input type="text" className="form-input" value={community.name} disabled />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                    Dirección Fiscal / Ubicación
                  </label>
                  <input type="text" className="form-input" value={community.address} disabled />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '4px' }}>
                    CIF de la Comunidad
                  </label>
                  <input type="text" className="form-input" value={community.cif || 'No especificado'} disabled />
                </div>
              </div>
            </div>
          )}
        </>
      )}

      {/* Add Member Modal */}
      {isAddMemberOpen && (
        <div className="modal-overlay">
          <div className="glass-panel modal-content" style={{ padding: '28px', maxWidth: '440px' }}>
            <h3 style={{ fontSize: '1.2rem', marginBottom: '16px' }}>Añadir Miembro al Censo</h3>
            <form onSubmit={handleAddMember} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                  Nombre *
                </label>
                <input
                  type="text"
                  className="form-input"
                  placeholder="Nombre del propietario / miembro"
                  value={newName}
                  onChange={e => setNewName(e.target.value)}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                  Apellidos
                </label>
                <input
                  type="text"
                  className="form-input"
                  placeholder="Apellidos"
                  value={newLastName}
                  onChange={e => setNewLastName(e.target.value)}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                  Correo Electrónico *
                </label>
                <input
                  type="email"
                  className="form-input"
                  placeholder="correo@ejemplo.com"
                  value={newEmail}
                  onChange={e => setNewEmail(e.target.value)}
                  required
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '6px' }}>
                  Rol en la Comunidad
                </label>
                <select
                  className="form-input"
                  value={newMemberRole}
                  onChange={e => setNewMemberRole(Number(e.target.value) as UserRole)}
                >
                  <option value={UserRole.CommunityMember}>Miembro Votante</option>
                  <option value={UserRole.CommunityAdmin}>Administrador de Comunidad</option>
                </select>
              </div>

              <div style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end', marginTop: '8px' }}>
                <button type="button" onClick={() => setIsAddMemberOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={addingMember} className="btn btn-primary">
                  {addingMember ? 'Añadiendo...' : 'Añadir al Censo'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Invitation Manager Modal */}
      {community && (
        <InvitationManagerModal
          communityId={community.id}
          communityName={community.name}
          isOpen={isInviteModalOpen}
          onClose={() => setIsInviteModalOpen(false)}
        />
      )}
    </div>
  );
};
