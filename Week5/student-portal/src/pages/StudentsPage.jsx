import React, { useState, useEffect } from 'react';
import StudentList from '../components/StudentList';
import api from '../api/axiosConfig';

const StudentsPage = () => {
  const [students, setStudents] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStudents = async () => {
      try {
        setLoading(true);
        // Fallback for when backend is not running to avoid complete breakage
        const response = await api.get('/students').catch(() => ({ data: [] }));
        setStudents(response.data);
      } catch (error) {
        console.error('Error fetching students:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchStudents();
  }, []);

  const filteredStudents = students.filter(student =>
    student.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    student.email?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const role = localStorage.getItem('role');

  return (
    <>
      <header className="dashboard-header">
        <h1>Student Portal</h1>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <span className="badge">Role: {role || 'Student'}</span>
          <button className="danger" onClick={() => { localStorage.clear(); window.location.href = '/login'; }}>Logout</button>
        </div>
      </header>

      <div className="dashboard-content">
        <div className="search-bar" style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <input
            type="text"
            placeholder="Search students by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <span style={{ color: 'var(--text-muted)', fontSize: '0.875rem', whiteSpace: 'nowrap' }}>
            {filteredStudents.length} matches
          </span>
        </div>
        
        <div className="glass-panel table-container">
          {loading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>Loading students...</div>
          ) : filteredStudents.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>No students found</div>
          ) : (
            <StudentList students={filteredStudents} role={role} />
          )}
        </div>
      </div>
    </>
  );
};

export default StudentsPage;
