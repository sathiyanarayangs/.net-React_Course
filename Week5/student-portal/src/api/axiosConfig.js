import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5237/api', // Points to Week 3 Auth API
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

export default api;
