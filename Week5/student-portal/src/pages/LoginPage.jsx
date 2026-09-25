import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/axiosConfig';
import { parseJwt } from '../utils/jwt';

const LoginPage = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    username: '',
    password: '',
  });
  const [error, setError] = useState('');

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await api.post('/auth/login', formData);
      const { token } = response.data;
      localStorage.setItem('token', token);
      
      const payload = parseJwt(token);
      if (payload && (payload.role || payload.Role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])) {
        const role = payload.role || payload.Role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        localStorage.setItem('role', role);
      }
      
      navigate('/students');
    } catch (err) {
      setError('Invalid credentials');
    }
  };

  return (
    <div className="auth-container glass-panel">
      <h2>Welcome Back</h2>
      {error && <p className="error-message">{error}</p>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="username">Username</label>
          <input type="text" id="username" name="username" value={formData.username} onChange={handleChange} required placeholder="e.g. alice.teacher" />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input type="password" id="password" name="password" value={formData.password} onChange={handleChange} required placeholder="••••••••" />
        </div>
        <button type="submit" style={{ width: '100%' }}>Login</button>
      </form>
      <p style={{ marginTop: '1.5rem', fontSize: '0.875rem', color: 'var(--text-muted)' }}>
        Don't have an account? <Link to="/register">Register here</Link>
      </p>
    </div>
  );
};

export default LoginPage;
