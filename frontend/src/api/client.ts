import axios from 'axios';

// Determine API URL based on environment
// - localhost: use explicit localhost:5002 for development
// - production (recommendo.norn.uk): use relative path (proxied by nginx)
const getApiUrl = () => {
  // Check if VITE_API_URL is explicitly set
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }
  
  // For localhost, use explicit API port
  if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    return 'http://localhost:5002';
  }
  
  // For production domains, use relative URL (nginx proxy)
  return '';
};

const API_URL = getApiUrl();

export const api = axios.create({
  baseURL: `${API_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
