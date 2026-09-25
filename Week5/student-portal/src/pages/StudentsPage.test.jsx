import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import StudentsPage from './StudentsPage';
import api from '../api/axiosConfig';

vi.mock('../api/axiosConfig');

describe('StudentsPage', () => {
  const mockStudents = [
    { id: 1, name: 'Alice', dob: '2000-01-01', designation: 'Student', email: 'alice@example.com' },
    { id: 2, name: 'Bob', dob: '2000-02-02', designation: 'Student', email: 'bob@example.com' }
  ];

  beforeEach(() => {
    api.get.mockResolvedValue({ data: mockStudents });
    localStorage.setItem('role', 'Teacher');
  });

  afterEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  test('fetches and displays students on mount', async () => {
    render(<StudentsPage />);
    expect(screen.getByText('Loading students...')).toBeInTheDocument();
    
    await waitFor(() => {
      expect(screen.getByText('Alice')).toBeInTheDocument();
    });
    expect(screen.getByText('Bob')).toBeInTheDocument();
    expect(screen.getByText('2 matches')).toBeInTheDocument();
  });

  test('search box filters live', async () => {
    render(<StudentsPage />);
    await waitFor(() => {
      expect(screen.getByText('Alice')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search students by name or email...');
    fireEvent.change(searchInput, { target: { value: 'Ali' } });
    
    expect(screen.getByText('Alice')).toBeInTheDocument();
    expect(screen.queryByText('Bob')).not.toBeInTheDocument();
    expect(screen.getByText('1 matches')).toBeInTheDocument();
  });

  test('shows empty state when no students match', async () => {
    render(<StudentsPage />);
    await waitFor(() => {
      expect(screen.getByText('Alice')).toBeInTheDocument();
    });

    const searchInput = screen.getByPlaceholderText('Search students by name or email...');
    fireEvent.change(searchInput, { target: { value: 'Zebra' } });
    
    expect(screen.getByText('No students found')).toBeInTheDocument();
    expect(screen.getByText('0 matches')).toBeInTheDocument();
  });
});
