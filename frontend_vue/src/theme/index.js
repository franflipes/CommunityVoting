export const COMMUNITY_THEMES = {
  communityDark: {
    dark: true,
    colors: {
      background: '#0f172a',
      surface: '#172033',
      primary: '#6366f1',
      secondary: '#22d3ee',
      error: '#ef4444',
      warning: '#f59e0b',
      success: '#22c55e',
      info: '#38bdf8'
    }
  },
  communityLight: {
    dark: false,
    colors: {
      background: '#f8fafc',
      surface: '#ffffff',
      primary: '#4f46e5',
      secondary: '#0891b2',
      error: '#dc2626',
      warning: '#d97706',
      success: '#16a34a',
      info: '#0284c7'
    }
  }
}

export function initialTheme() {
  return localStorage.getItem('cv_theme') || 'communityDark'
}

export function saveTheme(name) {
  localStorage.setItem('cv_theme', name)
  document.documentElement.dataset.theme = name
}
