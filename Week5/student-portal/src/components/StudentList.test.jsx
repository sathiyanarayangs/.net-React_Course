import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import StudentList from './StudentList';
import StudentsPage from '../pages/StudentsPage';

describe('StudentList', () => {
  const mockStudents = [
    { id: 1, name: 'Alice', dob: '2000-01-01', designation: 'Student', email: 'alice@example.com' },
    { id: 2, name: 'Bob', dob: '2000-02-02', designation: 'Student', email: 'bob@example.com' },
    { name: 'Charlie', dob: '2000-03-03', designation: 'Student', email: 'charlie@example.com' }
  ];

  test('renders rows from props', () => {
    render(<StudentList students={mockStudents} role="Student" />);
    expect(screen.getByText('Alice')).toBeInTheDocument();
    expect(screen.getByText('Bob')).toBeInTheDocument();
  });

  test('hides write controls for Student role', () => {
    render(<StudentList students={mockStudents} role="Student" />);
    expect(screen.queryByText('Edit')).not.toBeInTheDocument();
    expect(screen.queryByText('Delete')).not.toBeInTheDocument();
  });

  test('shows write controls for Teacher role', () => {
    render(<StudentList students={mockStudents} role="Teacher" />);
    expect(screen.getAllByText('Edit').length).toBe(3);
    expect(screen.getAllByText('Delete').length).toBe(3);
  });
});
