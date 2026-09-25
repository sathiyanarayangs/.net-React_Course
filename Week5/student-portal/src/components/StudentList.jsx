import React from 'react';
import StudentRow from './StudentRow';

const StudentList = ({ students, role }) => {
  return (
    <div className="student-list-container">
      <table className="student-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>DOB</th>
            <th>Designation</th>
            <th>Email</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {students.map((student, index) => (
            <StudentRow key={student.id || index} student={student} role={role} />
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default StudentList;
