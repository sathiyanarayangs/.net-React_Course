using StudentApi.Dtos;
using StudentApi.Models;
using StudentApi.Repositories;

namespace StudentApi.Services;

public interface ITeacherService
{
    IEnumerable<TeacherReadDto> GetAll();
    TeacherReadDto? GetById(int id);
    TeacherReadDto Create(TeacherCreateDto dto);
    bool Update(int id, TeacherCreateDto dto);
    bool Delete(int id);
}

public class TeacherService : ITeacherService
{
    private readonly IRepository<Teacher> _repository;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(IRepository<Teacher> repository, ILogger<TeacherService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public IEnumerable<TeacherReadDto> GetAll() => _repository.GetAll().Select(ToReadDto);

    public TeacherReadDto? GetById(int id)
    {
        var teacher = _repository.GetById(id);
        return teacher is null ? null : ToReadDto(teacher);
    }

    public TeacherReadDto Create(TeacherCreateDto dto)
    {
        var entity = new Teacher { Name = dto.Name, Subject = dto.Subject, Email = dto.Email };
        var created = _repository.Add(entity);
        _logger.LogInformation("Teacher {TeacherId} ({Name}) created.", created.Id, created.Name);
        return ToReadDto(created);
    }

    public bool Update(int id, TeacherCreateDto dto)
    {
        var existing = _repository.GetById(id);
        if (existing is null) return false;

        existing.Name = dto.Name;
        existing.Subject = dto.Subject;
        existing.Email = dto.Email;

        return _repository.Update(id, existing);
    }

    public bool Delete(int id) => _repository.Delete(id);

    private static TeacherReadDto ToReadDto(Teacher teacher) => new()
    {
        Id = teacher.Id,
        Name = teacher.Name,
        Subject = teacher.Subject,
        Email = teacher.Email
    };
}
