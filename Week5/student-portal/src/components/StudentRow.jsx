import React from 'react';

const StudentRow = ({ student, role }) => {
  return (
    <tr>
      <td>{student.name}</td>
      <td>{student.dob}</td>
      <td>{student.designation}</td>
      <td>{student.email}</td>
      <td>
        {role === 'Teacher' && (
          <div className="actions">
            <button className="edit-btn">Edit</button>
            <button className="delete-btn">Delete</button>
          </div>
        )}
      </td>
    </tr>
  );
};

export default StudentRow;
