using StudentApi.Models;

namespace StudentApi.Repositories;

public class StudentRepository : InMemoryRepository<Student>
{
    protected override int GetId(Student entity) => entity.Id;
    protected override void SetId(Student entity, int id) => entity.Id = id;
}

public class TeacherRepository : InMemoryRepository<Teacher>
{
    protected override int GetId(Teacher entity) => entity.Id;
    protected override void SetId(Teacher entity, int id) => entity.Id = id;
}
