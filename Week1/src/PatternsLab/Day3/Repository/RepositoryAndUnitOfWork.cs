namespace PatternsLab.Day3.Repository;

/// <summary>
/// Task 1.8 - Repository + Unit of Work. This is the seam every persistence
/// approach (in-memory here, EF Core/Dapper/ADO.NET later) plugs into: callers
/// depend only on IRepository&lt;T&gt; and IUnitOfWork, never on storage details.
/// </summary>
public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;

    public override string ToString() => $"Student#{Id} {Name} ({Course})";
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Credits { get; set; }

    public override string ToString() => $"Course#{Id} {Title} ({Credits} cr)";
}

/// <summary>
/// Minimal in-memory base repository. A real implementation would wrap
/// EF Core's DbSet&lt;T&gt; or raw ADO.NET/Dapper calls behind the same interface.
/// </summary>
public abstract class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly Dictionary<int, T> Store = new();
    protected abstract int GetId(T entity);
    protected abstract void SetId(T entity, int id);
    private int _nextId = 1;

    public IEnumerable<T> GetAll() => Store.Values;

    public T? GetById(int id) => Store.TryGetValue(id, out var entity) ? entity : null;

    public void Add(T entity)
    {
        SetId(entity, _nextId);
        Store[_nextId] = entity;
        _nextId++;
    }

    public void Update(T entity) => Store[GetId(entity)] = entity;

    public void Delete(int id) => Store.Remove(id);
}

public class StudentRepository : InMemoryRepository<Student>
{
    protected override int GetId(Student entity) => entity.Id;
    protected override void SetId(Student entity, int id) => entity.Id = id;
}

public class CourseRepository : InMemoryRepository<Course>
{
    protected override int GetId(Course entity) => entity.Id;
    protected override void SetId(Course entity, int id) => entity.Id = id;
}

/// <summary>
/// Unit of Work: exposes the repositories that participate in one logical
/// "transaction" and a single Save() that commits them together.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Student> Students { get; }
    IRepository<Course> Courses { get; }
    void Save();
}

public class UnitOfWork : IUnitOfWork
{
    public IRepository<Student> Students { get; }
    public IRepository<Course> Courses { get; }

    private readonly List<string> _pendingLog = new();

    public UnitOfWork(IRepository<Student> students, IRepository<Course> courses)
    {
        Students = students;
        Courses = courses;
    }

    public void Save()
    {
        // In-memory repos already write straight through; a real UoW would
        // flush a DbContext's change tracker or commit a DB transaction here.
        Console.WriteLine("[UnitOfWork] Save() committed all pending changes.");
    }
}

public static class RepositoryDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.8: Repository + Unit of Work ---");

        IUnitOfWork uow = new UnitOfWork(new StudentRepository(), new CourseRepository());

        uow.Students.Add(new Student { Name = "Priya", Course = "B.Tech CSE" });
        uow.Students.Add(new Student { Name = "Rahul", Course = "B.Tech ECE" });
        uow.Courses.Add(new Course { Title = "Distributed Systems", Credits = 4 });

        uow.Save();

        foreach (var student in uow.Students.GetAll())
            Console.WriteLine(student);
        foreach (var course in uow.Courses.GetAll())
            Console.WriteLine(course);
    }
}
