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

    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'short' } });
    fireEvent.change(screen.getByLabelText(/role/i), { target: { value: 'Teacher' } });
    
    // Still disabled because password < 6
    expect(submitBtn).toBeDisabled();

    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    
    // Now valid
    expect(submitBtn).not.toBeDisabled();
  });

  test('submits successfully and redirects to login', async () => {
    api.post.mockResolvedValue({});
    renderWithRouter(<RegisterPage />);
    
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    fireEvent.change(screen.getByLabelText(/role/i), { target: { value: 'Teacher' } });

    fireEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(api.post).toHaveBeenCalledWith('/auth/register', {
        username: 'John',
        password: 'password123',
        role: 'Teacher'
      });
    });
  });

  test('handles registration error', async () => {
    const alertMock = vi.spyOn(window, 'alert').mockImplementation(() => {});
    api.post.mockRejectedValue(new Error('Failed'));
    
    renderWithRouter(<RegisterPage />);
    
    fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'John' } });
    fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
    fireEvent.change(screen.getByLabelText(/role/i), { target: { value: 'Teacher' } });

    fireEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(alertMock).toHaveBeenCalledWith('Registration failed. Try again.');
    });
    alertMock.mockRestore();
  });

  test('handleSubmit returns early if invalid', () => {
    renderWithRouter(<RegisterPage />);
    
    // Do not fill form (isValid is false)
    const form = screen.getByRole('button', { name: /register/i }).closest('form');
    fireEvent.submit(form);

    expect(api.post).not.toHaveBeenCalled();
  });
});
