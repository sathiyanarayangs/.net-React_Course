using StudentApi.Dtos;
using StudentApi.Models;
using StudentApi.Patterns;
using StudentApi.Repositories;

namespace StudentApi.Services;

/// <summary>
/// Task 2.3/2.4 - all business logic lives here, not in the controller.
/// The controller only ever calls these methods and translates the result
/// into an HTTP response.
/// </summary>
public interface IStudentService
{
    IEnumerable<StudentReadDto> GetAll(string? gradeStrategyKey);
    StudentReadDto? GetById(int id, string? gradeStrategyKey);
    StudentReadDto Create(StudentCreateDto dto, string? gradeStrategyKey);
    bool Update(int id, StudentCreateDto dto);
    bool Delete(int id);
    IEnumerable<StudentReadDto> Search(string? name, string? gradeStrategyKey);
}

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _repository;
    private readonly IGradeStrategyFactory _gradeStrategyFactory;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IRepository<Student> repository,
        IGradeStrategyFactory gradeStrategyFactory,
        ILogger<StudentService> logger)
    {
        _repository = repository;
        _gradeStrategyFactory = gradeStrategyFactory;
        _logger = logger;
    }

    public IEnumerable<StudentReadDto> GetAll(string? gradeStrategyKey) =>
        _repository.GetAll().Select(s => ToReadDto(s, gradeStrategyKey));

    public StudentReadDto? GetById(int id, string? gradeStrategyKey)
    {
        var student = _repository.GetById(id);
        return student is null ? null : ToReadDto(student, gradeStrategyKey);
    }

    public StudentReadDto Create(StudentCreateDto dto, string? gradeStrategyKey)
    {
        var entity = new Student
        {
            Name = dto.Name,
            Age = dto.Age,
            Email = dto.Email,
            Score = dto.Score
            // InternalNotes intentionally left at its default — clients can never set it via the DTO.
        };

        var created = _repository.Add(entity);
        _logger.LogInformation("Student {StudentId} ({Name}) created.", created.Id, created.Name);
        return ToReadDto(created, gradeStrategyKey);
    }

    public bool Update(int id, StudentCreateDto dto)
    {
        var existing = _repository.GetById(id);
        if (existing is null) return false;

        existing.Name = dto.Name;
        existing.Age = dto.Age;
        existing.Email = dto.Email;
        existing.Score = dto.Score;

        return _repository.Update(id, existing);
    }

    public bool Delete(int id) => _repository.Delete(id);

    public IEnumerable<StudentReadDto> Search(string? name, string? gradeStrategyKey)
    {
        var matches = string.IsNullOrWhiteSpace(name)
            ? _repository.GetAll()
            : _repository.GetAll().Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        return matches.Select(s => ToReadDto(s, gradeStrategyKey));
    }

    private StudentReadDto ToReadDto(Student student, string? gradeStrategyKey)
    {
        var strategy = _gradeStrategyFactory.GetStrategy(gradeStrategyKey);

        return new StudentReadDto
        {
            Id = student.Id,
            Name = student.Name,
            Age = student.Age,
            Email = student.Email,
            Score = student.Score,
            Grade = strategy.CalculateGrade(student.Score)
            // Note: InternalNotes is never read here — StudentReadDto has no
            // such property, so there is no way for it to leak.
        };
    }
}
