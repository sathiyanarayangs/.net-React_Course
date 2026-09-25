import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axiosConfig';

const RegisterPage = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: '',
    dob: '',
    designation: '',
    email: '',
    password: '',
  });

  const isValid = formData.name && formData.dob && formData.designation && formData.email && formData.password.length >= 6;

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isValid) return;

    try {
      await api.post('/auth/register', formData);
      navigate('/login');
    } catch (error) {
      console.error('Registration failed:', error);
      alert('Registration failed. Try again.');
    }
  };

  return (
    <div className="auth-container glass-panel">
      <h2>Create an Account</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input type="text" id="name" name="name" value={formData.name} onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="dob">Date of Birth</label>
          <input type="date" id="dob" name="dob" value={formData.dob} onChange={handleChange} required />
        </div>
        <div className="form-group">
          <label htmlFor="designation">Designation</label>
          <input type="text" id="designation" name="designation" value={formData.designation} onChange={handleChange} required placeholder="e.g. Student" />
        </div>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input type="email" id="email" name="email" value={formData.email} onChange={handleChange} required placeholder="you@example.com" />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password (Min 6 chars)</label>
          <input type="password" id="password" name="password" value={formData.password} onChange={handleChange} required placeholder="••••••••" />
        </div>
        <button type="submit" disabled={!isValid} style={{ width: '100%', marginTop: '1rem' }}>Register</button>
      </form>
    </div>
  );
};

export default RegisterPage;
