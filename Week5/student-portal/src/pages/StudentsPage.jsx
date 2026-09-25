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
    <div className="students-page">
      <header>
        <h1>Student Portal</h1>
      </header>
      <div className="search-container">
        <input
          type="text"
          placeholder="Search students..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <span className="match-count">{filteredStudents.length} matches</span>
      </div>
      
      {loading ? (
        <div className="loading">Loading students...</div>
      ) : filteredStudents.length === 0 ? (
        <div className="empty-state">No students found</div>
      ) : (
        <StudentList students={filteredStudents} role={role} />
      )}
    </div>
  );
};

export default StudentsPage;
