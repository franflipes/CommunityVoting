import React from 'react';
import { Navbar } from './Navbar';

export const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Navbar />
      <main style={{ flex: 1, maxWidth: '1200px', width: '100%', margin: '0 auto', padding: '32px 24px' }}>
        {children}
      </main>
      <footer style={{ borderTop: '1px solid var(--border-color)', padding: '24px 0', textAlign: 'center', fontSize: '0.85rem', color: 'var(--text-dim)' }}>
        CommunityVoting &copy; 2026. Sistema Integrado de Votaciones de Comunidad.
      </footer>
    </div>
  );
};
