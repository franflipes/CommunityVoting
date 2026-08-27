import axios from 'axios';

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5004/api';
export const VOTING_API_BASE_URL = import.meta.env.VITE_VOTING_API_BASE_URL || 'http://localhost:5222/api/voting';
export const DOCUMENT_API_BASE_URL = import.meta.env.VITE_DOCUMENT_API_BASE_URL || 'http://localhost:5088/api';
export const VOTING_HUB_URL = import.meta.env.VITE_VOTING_HUB_URL || 'http://localhost:5222/hubs/voting';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
});

export const votingApiClient = axios.create({
  baseURL: VOTING_API_BASE_URL,
});

export const documentApiClient = axios.create({
  baseURL: DOCUMENT_API_BASE_URL,
});

const attachToken = (config: any) => {
  const token = localStorage.getItem('cv_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
};

apiClient.interceptors.request.use(attachToken);
votingApiClient.interceptors.request.use(attachToken);
documentApiClient.interceptors.request.use(attachToken);
