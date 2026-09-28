using DataAccess.Core.Models;
using DataAccess.Core.Repositories;

namespace DataAccess.Core.Services;

/// <summary>
/// The whole point of the repository seam: this class depends only on
/// IRepository&lt;Student&gt;, never on AdoNetStudentRepository,
/// EfStudentRepository, or the DB First one specifically. Task 4.11's
/// config swap works BECAUSE this class was written against the
/// interface, not a concrete class — and that's exactly what the
/// mocked-repository unit tests below prove: the service's logic is
/// identical no matter which real repository would be behind it in
/// production.
/// </summary>
public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<bool> UpdateAsync(int id, Student student);
    Task<bool> DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;

    public StudentService(IRepository<Student> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Student>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Student?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<Student> CreateAsync(Student student) => _repository.AddAsync(student);

    public Task<bool> UpdateAsync(int id, Student student) => _repository.UpdateAsync(id, student);

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
