import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { MeetingAccessPage } from './pages/MeetingAccessPage';
import { CommunitiesPage } from './pages/CommunitiesPage';
import { CommunityDetailPage } from './pages/CommunityDetailPage';
import { MeetingDetailPage } from './pages/MeetingDetailPage';
import { VotingRoomPage } from './pages/VotingRoomPage';
import { AdminSettingsPage } from './pages/AdminSettingsPage';
import { JoinCommunityPage } from './pages/JoinCommunityPage';

const queryClient = new QueryClient();

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();
  if (loading) return <div style={{ textAlign: 'center', padding: '60px', color: 'var(--text-muted)' }}>Cargando aplicación...</div>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <Layout>{children}</Layout>;
};

export const App: React.FC = () => {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/meeting/access/:token" element={<MeetingAccessPage />} />
            <Route path="/join" element={<JoinCommunityPage />} />

            <Route
              path="/communities"
              element={
                <ProtectedRoute>
                  <CommunitiesPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/communities/:id"
              element={
                <ProtectedRoute>
                  <CommunityDetailPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/communities/:id/settings"
              element={
                <ProtectedRoute>
                  <AdminSettingsPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/admin/settings"
              element={
                <ProtectedRoute>
                  <AdminSettingsPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/meetings/:id"
              element={
                <ProtectedRoute>
                  <MeetingDetailPage />
                </ProtectedRoute>
              }
            />

            <Route
              path="/voting/:sessionId"
              element={
                <ProtectedRoute>
                  <VotingRoomPage />
                </ProtectedRoute>
              }
            />

            <Route path="*" element={<Navigate to="/communities" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
};

export default App;
