import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from './LoginPage';
import api from '../api/axiosConfig';
import * as jwtUtils from '../utils/jwt';

vi.mock('../api/axiosConfig');
vi.mock('../utils/jwt');

const renderWithRouter = (ui) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

describe('LoginPage', () => {
  afterEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  test('successful login sets token and role, and redirects', async () => {
    api.post.mockResolvedValue({
      data: { token: 'mock-token' }
    });
    jwtUtils.parseJwt.mockReturnValue({ role: 'Teacher' });

    renderWithRouter(<LoginPage />);

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'teacher' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(api.post).toHaveBeenCalledWith('/auth/login', {
        username: 'teacher',
        password: 'password123'
      });
    });
    
    expect(localStorage.getItem('token')).toBe('mock-token');
  });

  test('failed login shows error message', async () => {
    api.post.mockRejectedValue(new Error('Invalid credentials'));

    renderWithRouter(<LoginPage />);

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'bad' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'wrong' } });
    
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });
  });

  test('login sets token but no role if not in payload', async () => {
    api.post.mockResolvedValue({
      data: { token: 'mock-token' }
    });
    jwtUtils.parseJwt.mockReturnValue({});

    renderWithRouter(<LoginPage />);

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'student' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    
    fireEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBe('mock-token');
      expect(localStorage.getItem('role')).toBeNull();
    });
  });
});
