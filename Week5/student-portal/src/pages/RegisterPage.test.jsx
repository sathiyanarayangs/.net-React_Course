import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import RegisterPage from './RegisterPage';
import api from '../api/axiosConfig';

vi.mock('../api/axiosConfig');

const renderWithRouter = (ui) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

describe('RegisterPage', () => {
  test('controlled form disables submit until valid', () => {
    renderWithRouter(<RegisterPage />);
    const submitBtn = screen.getByRole('button', { name: /register/i });
    expect(submitBtn).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/name/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/date of birth/i), { target: { value: '2000-01-01' } });
    fireEvent.change(screen.getByLabelText(/designation/i), { target: { value: 'Student' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'john@example.com' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'short' } });
    
    // Still disabled because password < 6
    expect(submitBtn).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    
    // Now valid
    expect(submitBtn).not.toBeDisabled();
  });

  test('submits successfully and redirects to login', async () => {
    api.post.mockResolvedValue({});
    renderWithRouter(<RegisterPage />);
    
    fireEvent.change(screen.getByLabelText(/name/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/date of birth/i), { target: { value: '2000-01-01' } });
    fireEvent.change(screen.getByLabelText(/designation/i), { target: { value: 'Student' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'john@example.com' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });

    fireEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(api.post).toHaveBeenCalledWith('/auth/register', {
        name: 'John',
        dob: '2000-01-01',
        designation: 'Student',
        email: 'john@example.com',
        password: 'password123'
      });
    });
  });

  test('handles registration error', async () => {
    const alertMock = vi.spyOn(window, 'alert').mockImplementation(() => {});
    api.post.mockRejectedValue(new Error('Failed'));
    
    renderWithRouter(<RegisterPage />);
    
    fireEvent.change(screen.getByLabelText(/name/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/date of birth/i), { target: { value: '2000-01-01' } });
    fireEvent.change(screen.getByLabelText(/designation/i), { target: { value: 'Student' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'john@example.com' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });

    fireEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(alertMock).toHaveBeenCalledWith('Registration failed. Try again.');
    });
    alertMock.mockRestore();
  });
});
